using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class TransferInitiatedEventHandler : INotificationHandler<TransferInitiatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPapssService _papssService;
    private readonly IMediator _mediator;
    private readonly ILogger<TransferInitiatedEventHandler> _logger;

    public TransferInitiatedEventHandler(
        IUnitOfWork unitOfWork,
        IPapssService papssService,
        IMediator mediator,
        ILogger<TransferInitiatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _papssService = papssService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(TransferInitiatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing TransferInitiatedEvent for Transfer: {TransferId}", notification.TransferId.Value);

        try
        {
            // Get the transfer
            var transfer = await _unitOfWork.Transfers.GetByIdAsync(notification.TransferId, cancellationToken);
            if (transfer == null)
            {
                _logger.LogError("Transfer not found: {TransferId}", notification.TransferId.Value);
                return;
            }

            // If transfer is already in a terminal state, skip re-processing to keep idempotency
            if (transfer.Status == TransferStatus.Completed || transfer.Status == TransferStatus.Failed)
            {
                _logger.LogInformation(
                    "Skipping TransferInitiatedEvent for Transfer {TransferId} in terminal status {Status}",
                    transfer.Id.Value,
                    transfer.Status);
                return;
            }

            // Mark transfer as processing
            transfer.MarkAsProcessing();
            _unitOfWork.Transfers.Update(transfer);

            // Get source and destination accounts
            var sourceAccount = await _unitOfWork.Accounts.GetByIdAsync(transfer.SourceAccountId, cancellationToken);
            var destinationAccount = await _unitOfWork.Accounts.GetByIdAsync(transfer.DestinationAccountId, cancellationToken);

            if (sourceAccount == null || destinationAccount == null)
            {
                _logger.LogError("Account not found. Source: {SourceExists}, Destination: {DestExists}",
                    sourceAccount != null, destinationAccount != null);
                transfer.Fail("Account not found");
                _unitOfWork.Transfers.Update(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            // Determine whether PAPSS is required based on customers' identity documents
            var sourceCustomer = await _unitOfWork.Customers.GetByIdAsync(sourceAccount.CustomerId, cancellationToken);
            var destinationCustomer = await _unitOfWork.Customers.GetByIdAsync(destinationAccount.CustomerId, cancellationToken);

            if (sourceCustomer == null || destinationCustomer == null)
            {
                _logger.LogError("Customer not found for transfer. Source: {SourceExists}, Destination: {DestExists}",
                    sourceCustomer != null, destinationCustomer != null);
                transfer.Fail("Customer not found");
                _unitOfWork.Transfers.Update(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            var (sourceCurrency, destinationCurrency) = ResolveCurrenciesForPapss(sourceCustomer, destinationCustomer, transfer.Amount.Currency);

            PapssSettlementResult? papssResult = null;
            // Default debit/credit amounts are as captured on the transfer
            var debitAmount = transfer.TotalDebitAmount;
            var creditAmount = transfer.Amount;

            if (!string.Equals(sourceCurrency, destinationCurrency, StringComparison.OrdinalIgnoreCase))
            {
                // Cross-country / cross-currency transfer – go through PAPSS
                var papssRequest = new PapssSettlementRequest
                {
                    TransferId = transfer.Id.Value,
                    SourceCurrency = sourceCurrency,
                    DestinationCurrency = destinationCurrency,
                    Amount = transfer.Amount.Amount
                };

                var settlement = await _papssService.SettleAsync(papssRequest, cancellationToken);
                if (!settlement.IsSuccess)
                {
                    var failureReason = settlement.Error ?? "PAPSS settlement failed";
                    _logger.LogWarning("PAPSS settlement failed for Transfer {TransferId}: {Reason}", transfer.Id.Value, failureReason);

                    transfer.Fail(failureReason);
                    _unitOfWork.Transfers.Update(transfer);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }

                papssResult = settlement.Value;

                if (papssResult.RequiresPapss && !papssResult.IsSuccess)
                {
                    var reason = papssResult.FailureReason ?? "PAPSS settlement declined";
                    transfer.Fail(reason);
                    _unitOfWork.Transfers.Update(transfer);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return;
                }

                // After successful PAPSS settlement, determine the effective
                // source (debit) and destination (credit) amounts without
                // reassigning the owned Money navigations. This avoids EF Core
                // tracking issues with the owned Money value objects.
                debitAmount = new AfriPay.CORE.ValueObjects.Money(
                    papssResult.SourceAmount,
                    papssResult.SourceCurrency);
                creditAmount = new AfriPay.CORE.ValueObjects.Money(
                    papssResult.DestinationAmount,
                    papssResult.DestinationCurrency);

                // Fire a PAPSS-specific notification for observability / downstream integration
                await _mediator.Publish(new PapssSettlementCompletedNotification(
                    transfer.Id.Value,
                    papssResult.SourceCurrency,
                    papssResult.DestinationCurrency,
                    papssResult.SourceAmount,
                    papssResult.DestinationAmount,
                    papssResult.FxRate),
                    cancellationToken);
            }

            // Decide the actual debit/credit amounts after any PAPSS adjustment.

            // Debit source account
            var debitResult = sourceAccount.Debit(
                debitAmount,
                transfer.TransferReference,
                $"Transfer to {transfer.DestinationUserTag ?? destinationAccount.AccountNumber.Value}"
            );

            if (!debitResult.IsSuccess)
            {
                _logger.LogWarning("Failed to debit source account. Reason: {Reason}", debitResult.Error);
                transfer.Fail(debitResult.Error);
                _unitOfWork.Transfers.Update(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            // Credit destination account
            var creditResult = destinationAccount.Credit(
                creditAmount,
                transfer.TransferReference,
                $"Transfer from {sourceAccount.AccountNumber.Value}"
            );

            if (!creditResult.IsSuccess)
            {
                _logger.LogError("Failed to credit destination account. Reason: {Reason}. CRITICAL: Need to reverse debit!", creditResult.Error);

                // Reverse the debit
                sourceAccount.Credit(transfer.TotalDebitAmount, transfer.TransferReference, "Reversal");

                transfer.Fail($"Credit failed: {creditResult.Error}");
                _unitOfWork.Transfers.Update(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            // EF Core tracks changes automatically, no need to call Update

            // Create transaction records
            // Note: Balance before = current balance + amount debited (to get balance before the debit)
            var sourceBalanceBefore = sourceAccount.Balance.Amount + transfer.TotalDebitAmount.Amount;
            var debitTransaction = AfriPay.CORE.Entities.Transaction.CreateDebit(
                transfer.SourceAccountId,
                transfer.SourceCustomerId,
                debitAmount,
                sourceBalanceBefore,
                transfer.Narration ?? "Transfer",
                transfer.Id
            );

            // Balance before = current balance - amount credited (to get balance before the credit)
            var destBalanceBefore = destinationAccount.Balance.Amount - creditAmount.Amount;
            var creditTransaction = AfriPay.CORE.Entities.Transaction.CreateCredit(
                transfer.DestinationAccountId,
                transfer.DestinationCustomerId,
                creditAmount,
                destBalanceBefore,
                transfer.Narration ?? "Transfer",
                transfer.Id
            );

            await _unitOfWork.Transactions.AddAsync(debitTransaction, cancellationToken);
            await _unitOfWork.Transactions.AddAsync(creditTransaction, cancellationToken);

            // Complete the transfer
            transfer.Complete();
            _unitOfWork.Transfers.Update(transfer);

            // Save all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Transfer processed successfully: {TransferId}, Reference: {Reference}",
                transfer.Id.Value, transfer.TransferReference);

            // Note: SignalR notifications should be sent by a separate handler in the API layer
            // that listens to TransferCompletedEvent
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer: {TransferId}", notification.TransferId.Value);

            // Mark transfer as failed
            var transfer = await _unitOfWork.Transfers.GetByIdAsync(notification.TransferId, cancellationToken);
            if (transfer != null)
            {
                transfer.Fail($"System error: {ex.Message}");
                _unitOfWork.Transfers.Update(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Lightweight mapping from customer identity documents to their home currency.
    /// This allows us to infer when a transfer is cross-country and should go through PAPSS.
    /// </summary>
    private static (string SourceCurrency, string DestinationCurrency) ResolveCurrenciesForPapss(
        Customer sourceCustomer,
        Customer destinationCustomer,
        string fallbackCurrency)
    {
        // Today we infer country from the primary identity type.
        // This can be refined later (e.g. explicit Country on Customer) without changing call sites.
        static string MapIdentityToCurrency(IdentityType identityType, string defaultCurrency) => identityType switch
        {
            IdentityType.BVN => "NGN",            // Nigeria
            IdentityType.GhanaCard => "GHS",      // Ghana
            IdentityType.KenyaNationalID => "KES",// Kenya
            _ => defaultCurrency                   // NIN/Passport -> treat as current ledger currency
        };

        var sourceCurrency = MapIdentityToCurrency(sourceCustomer.IdentityType, fallbackCurrency);
        var destinationCurrency = MapIdentityToCurrency(destinationCustomer.IdentityType, fallbackCurrency);

        return (sourceCurrency, destinationCurrency);
    }
}
