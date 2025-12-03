using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class TransferInitiatedEventHandler : INotificationHandler<TransferInitiatedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransferInitiatedEventHandler> _logger;

    public TransferInitiatedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<TransferInitiatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
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

            // Debit source account
            var debitResult = sourceAccount.Debit(
                transfer.TotalDebitAmount,
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
                transfer.Amount,
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
                transfer.TotalDebitAmount,
                sourceBalanceBefore,
                transfer.Narration ?? "Transfer",
                transfer.Id
            );

            // Balance before = current balance - amount credited (to get balance before the credit)
            var destBalanceBefore = destinationAccount.Balance.Amount - transfer.Amount.Amount;
            var creditTransaction = AfriPay.CORE.Entities.Transaction.CreateCredit(
                transfer.DestinationAccountId,
                transfer.DestinationCustomerId,
                transfer.Amount,
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
}