using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Transfers.Commands.InitiateTransfer;

public class InitiateTransferCommandHandler : IRequestHandler<InitiateTransferCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitiateTransferCommandHandler> _logger;

    public InitiateTransferCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<InitiateTransferCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(InitiateTransferCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating transfer from account {SourceAccountId} to {DestinationAccountId}, Amount: {Amount}",
            request.SourceAccountId, request.DestinationAccountId, request.Amount);

        // 1. Check for duplicate transfer (idempotency)
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingTransfer = await _unitOfWork.Transfers.GetByIdempotencyKeyAsync(
                request.IdempotencyKey, cancellationToken);

            if (existingTransfer != null)
            {
                _logger.LogWarning("Duplicate transfer request detected. IdempotencyKey: {IdempotencyKey}, Returning existing TransferId: {TransferId}",
                    request.IdempotencyKey, existingTransfer.Id.Value);
                return Result<Guid>.Success(existingTransfer.Id.Value);
            }
        }

        // 2. Validate source account exists
        var sourceAccountId = AccountId.Create(request.SourceAccountId);
        var sourceAccount = await _unitOfWork.Accounts.GetByIdAsync(sourceAccountId, cancellationToken);

        if (sourceAccount == null)
        {
            _logger.LogWarning("Source account not found: {SourceAccountId}", request.SourceAccountId);
            return Result<Guid>.Failure("Source account not found");
        }

        // 3. Resolve destination account (by AccountId or UserTag)
        Account? destinationAccount = null;

        if (!string.IsNullOrWhiteSpace(request.DestinationUserTag))
        {
            // Resolve by UserTag
            var normalizedTag = request.DestinationUserTag.TrimStart('@').ToUpperInvariant();
            var destinationCustomer = await _unitOfWork.Customers.GetByUserTagAsync(normalizedTag, cancellationToken);

            if (destinationCustomer == null)
            {
                _logger.LogWarning("Destination customer not found with UserTag: {UserTag}", request.DestinationUserTag);
                return Result<Guid>.Failure($"User with tag '{request.DestinationUserTag}' not found");
            }

            // Get the customer's first account (in a real app, this would be the primary account)
            var accounts = await _unitOfWork.Accounts.GetByCustomerIdAsync(
                destinationCustomer.CustomerId, cancellationToken);
            destinationAccount = accounts.FirstOrDefault();

            if (destinationAccount == null)
            {
                _logger.LogWarning("Destination customer has no accounts: {CustomerId}", destinationCustomer.CustomerId);
                return Result<Guid>.Failure($"User '{request.DestinationUserTag}' has no active account");
            }
        }
        else
        {
            // Resolve by AccountId
            var destinationAccountId = AccountId.Create(request.DestinationAccountId);
            destinationAccount = await _unitOfWork.Accounts.GetByIdAsync(destinationAccountId, cancellationToken);

            if (destinationAccount == null)
            {
                _logger.LogWarning("Destination account not found: {DestinationAccountId}", request.DestinationAccountId);
                return Result<Guid>.Failure("Destination account not found");
            }
        }

        // 4. Validate sufficient balance
        var currency = request.Currency ?? "NGN";
        var amount = new Money(request.Amount, currency);

        // For internal transfers, no fee for now (can be added later)
        Money? fee = null;
        var totalDebit = amount;

        var availableBalance = sourceAccount.GetAvailableBalance();
        if (availableBalance < totalDebit.Amount)
        {
            _logger.LogWarning("Insufficient balance. Available: {Available}, Required: {Required}",
                availableBalance, totalDebit.Amount);
            return Result<Guid>.Failure($"Insufficient balance. Available: {availableBalance:N2}, Required: {totalDebit.Amount:N2}");
        }

        // 5. Create transfer entity
        var transfer = Transfer.Create(
            sourceAccountId,
            sourceAccount.CustomerId,
            destinationAccount.AccountId,
            destinationAccount.CustomerId,
            amount,
            fee,
            TransferType.Internal,
            request.Description,
            request.DestinationUserTag,
            request.IdempotencyKey
        );

        await _unitOfWork.Transfers.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);  // This dispatches TransferInitiatedEvent

        _logger.LogInformation("Transfer initiated successfully. TransferId: {TransferId}, Reference: {Reference}",
            transfer.Id.Value, transfer.TransferReference);

        return Result<Guid>.Success(transfer.Id.Value);
    }
}
