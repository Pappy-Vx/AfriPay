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
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<InitiateTransferCommandHandler> _logger;

    public InitiateTransferCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<InitiateTransferCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
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

        // 2b. Verify transfer PIN against the source customer
        var sourceCustomer = await _unitOfWork.Customers.GetByIdAsync(sourceAccount.CustomerId, cancellationToken);
        if (sourceCustomer == null)
        {
            _logger.LogWarning("Source customer not found for account {SourceAccountId}", request.SourceAccountId);
            return Result<Guid>.Failure("Source customer not found");
        }

        if (string.IsNullOrEmpty(sourceCustomer.TransferPinHash))
        {
            _logger.LogWarning("Transfer PIN not set for customer {CustomerId}", sourceCustomer.CustomerId);
            return Result<Guid>.Failure("Transfer PIN is not set for this customer");
        }

        var isPinValid = _passwordHasher.VerifyPassword(request.Pin, sourceCustomer.TransferPinHash);
        if (!isPinValid)
        {
            _logger.LogWarning("Invalid transfer PIN for customer {CustomerId}", sourceCustomer.CustomerId);
            return Result<Guid>.Failure("Invalid transfer PIN");
        }

        // 3. Resolve destination account (by AccountId and/or UserTag)
        Account? destinationAccount = null;

        var hasTag = !string.IsNullOrWhiteSpace(request.DestinationUserTag);
        var hasAccountId = request.DestinationAccountId != Guid.Empty;

        if (!hasTag && !hasAccountId)
        {
            _logger.LogWarning("No destination provided. Either DestinationAccountId or DestinationUserTag is required.");
            return Result<Guid>.Failure("Either destinationAccountId or destinationUserTag is required");
        }

        if (hasTag)
        {
            // Resolve by UserTag
            var normalizedTag = request.DestinationUserTag.TrimStart('@').ToUpperInvariant();
            var destinationCustomer = await _unitOfWork.Customers.GetByUserTagAsync(normalizedTag, cancellationToken);

            if (destinationCustomer == null)
            {
                _logger.LogWarning("Destination customer not found with UserTag: {UserTag}", request.DestinationUserTag);
                return Result<Guid>.Failure($"User with tag '{request.DestinationUserTag}' not found");
            }

            // Get all accounts for that customer
            var accounts = await _unitOfWork.Accounts.GetByCustomerIdAsync(
                destinationCustomer.CustomerId, cancellationToken);

            if (!accounts.Any())
            {
                _logger.LogWarning("Destination customer has no accounts: {CustomerId}", destinationCustomer.CustomerId);
                return Result<Guid>.Failure($"User '{request.DestinationUserTag}' has no active account");
            }

            if (hasAccountId)
            {
                // Both tag and accountId were supplied – ensure they are consistent
                destinationAccount = accounts.FirstOrDefault(a => a.AccountId.Value == request.DestinationAccountId);
                if (destinationAccount == null)
                {
                    _logger.LogWarning(
                        "Destination account {DestinationAccountId} does not belong to user tag {UserTag}",
                        request.DestinationAccountId,
                        request.DestinationUserTag);
                    return Result<Guid>.Failure(
                        "Destination account does not belong to the specified user tag");
                }
            }
            else
            {
                // Only tag provided – default to the first (e.g. primary) account
                destinationAccount = accounts.First();
            }
        }
        else
        {
            // Only AccountId provided – resolve by AccountId and ensure its customer has a UserTag
            var destinationAccountId = AccountId.Create(request.DestinationAccountId);
            destinationAccount = await _unitOfWork.Accounts.GetByIdAsync(destinationAccountId, cancellationToken);

            if (destinationAccount == null)
            {
                _logger.LogWarning("Destination account not found: {DestinationAccountId}", request.DestinationAccountId);
                return Result<Guid>.Failure("Destination account not found");
            }

            var destinationCustomer = await _unitOfWork.Customers.GetByIdAsync(destinationAccount.CustomerId, cancellationToken);
            if (destinationCustomer == null)
            {
                _logger.LogWarning("Destination customer not found for account {DestinationAccountId}", request.DestinationAccountId);
                return Result<Guid>.Failure("Destination customer not found");
            }

            if (destinationCustomer.UserTag is null)
            {
                _logger.LogWarning("Destination account {DestinationAccountId} has no associated UserTag", request.DestinationAccountId);
                return Result<Guid>.Failure("Destination account has no associated user tag");
            }
        }

        // Prevent transfers to the same account
        if (destinationAccount.AccountId == sourceAccount.AccountId)
        {
            _logger.LogWarning("Attempted transfer to the same account: {AccountId}", sourceAccount.AccountId.Value);
            return Result<Guid>.Failure("Cannot transfer to the same account");
        }

        // 4. Validate transfer limits and sufficient balance
        var currency = sourceAccount.Balance.Currency;
        var amount = new Money(request.Amount, currency);

        // Enforce single transfer limit
        if (request.Amount > sourceAccount.SingleTransferLimit)
        {
            _logger.LogWarning(
                "Amount exceeds single transfer limit. Limit: {Limit}, Amount: {Amount}",
                sourceAccount.SingleTransferLimit,
                request.Amount);
            return Result<Guid>.Failure($"Amount exceeds single transfer limit of {sourceAccount.SingleTransferLimit}");
        }

        // Enforce daily transfer limit (simulate ResetDailyLimitIfNeeded logic)
        var today = DateTime.UtcNow.Date;
        var dailyTotal = sourceAccount.DailyTransferTotal;
        if (sourceAccount.LastDailyResetDate < today)
        {
            dailyTotal = 0;
        }

        if (dailyTotal + request.Amount > sourceAccount.DailyTransferLimit)
        {
            _logger.LogWarning(
                "Amount exceeds daily transfer limit. Limit: {Limit}, CurrentTotal: {CurrentTotal}, Requested: {Requested}",
                sourceAccount.DailyTransferLimit,
                dailyTotal,
                request.Amount);
            return Result<Guid>.Failure($"Amount exceeds daily transfer limit of {sourceAccount.DailyTransferLimit}");
        }

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