# Transfer System Documentation

## Overview
The Transfer system handles money transfers between AfriPay accounts (internal transfers) and to external banks. It's built using Domain-Driven Design (DDD) with CQRS pattern, MediatR, and domain events.

---

## ✅ Current Status

### What's Working
1. **Database Schema** - Successfully migrated with all tables and relationships
2. **Domain Model** - Transfer entity with proper value objects (TransferId, Money, etc.)
3. **Repository Pattern** - ITransferRepository and TransferRepository implemented
4. **Domain Events** - TransferInitiatedEvent, TransferCompletedEvent, TransferFailedEvent
5. **EF Core Configuration** - Proper value converters for all value objects
6. **DbContext** - Updated to support Transfer aggregate root domain events

### What's Missing (Needs Implementation)
1. **Controller** - API endpoints for transfers
2. **Commands/Queries** - MediatR command and query handlers
3. **Event Handlers** - Handlers for domain events (to create transactions)
4. **Validation** - FluentValidation validators for transfer operations
5. **Business Logic** - Account balance checking, transfer processing service

---

## Architecture

### Domain Layer (AfriPay.CORE)

#### Transfer Entity
Location: [AfriPay.CORE/Entities/Transfer.cs](AfriPay.CORE/Entities/Transfer.cs)

```csharp
public class Transfer : AggregateRoot<TransferId>
{
  // Inherits Id property from AggregateRoot<TransferId>
  public string TransferReference { get; private set; }
  public string? IdempotencyKey { get; private set; }

  // Source
  public AccountId SourceAccountId { get; private set; }
  public CustomerId SourceCustomerId { get; private set; }

  // Destination
  public AccountId DestinationAccountId { get; private set; }
  public CustomerId DestinationCustomerId { get; private set; }
  public string? DestinationUserTag { get; private set; }

  // Money
  public Money Amount { get; private set; }
  public Money? Fee { get; private set; }
  public Money TotalDebitAmount { get; private set; }

  // Status
  public TransferType Type { get; private set; }  // Internal, External
  public TransferStatus Status { get; private set; }  // Pending, Processing, Completed, Failed
  public string? Description { get; private set; }
  public string? Narration { get; private set; }
  public string? FailureReason { get; private set; }

  // Timestamps
  public DateTime CreatedAt { get; private set; }
  public DateTime? CompletedAt { get; private set; }
}
```

**Key Methods:**
- `Transfer.Create()` - Factory method to create a new transfer
- `MarkAsProcessing()` - Move transfer from Pending to Processing
- `Complete()` - Mark transfer as completed (raises TransferCompletedEvent)
- `Fail(reason)` - Mark transfer as failed (raises TransferFailedEvent)

**Validations:**
- Cannot transfer to the same account
- Amount must be greater than zero
- Can only complete a transfer in Processing status
- Can only process a transfer in Pending status

#### Value Objects
- **TransferId**: Strongly-typed ID using Guid
- **Money**: Amount + Currency (stored as owned entity)
- **AccountId**, **CustomerId**: Value objects for entity references

#### Enums
- **TransferType**: Internal (1), External (2)
- **TransferStatus**: Pending (1), Processing (2), Completed (3), Failed (4), Reversed (5)

#### Domain Events
1. **TransferInitiatedEvent** - Raised when transfer is created
2. **TransferCompletedEvent** - Raised when transfer completes successfully
3. **TransferFailedEvent** - Raised when transfer fails

---

### Data Layer (AfriPay.DAL)

#### TransferConfiguration
Location: [AfriPay.DAL/Configurations/TransferConfiguration.cs](AfriPay.DAL/Configurations/TransferConfiguration.cs)

**Key Configurations:**
- `Id` property uses value converter to store as Guid
- All value objects (AccountId, CustomerId) use value converters
- Money objects (Amount, Fee, TotalDebitAmount) stored as owned entities
- Unique indexes on TransferReference and IdempotencyKey
- Indexes on SourceCustomerId, DestinationCustomerId, CreatedAt

#### TransferRepository
Location: [AfriPay.DAL/Repositories/TransferRepository.cs](AfriPay.DAL/Repositories/TransferRepository.cs)

**Available Methods:**
```csharp
Task<Transfer?> GetByIdAsync(TransferId id, CancellationToken cancellationToken = default);
Task<Transfer?> GetByReferenceAsync(string transferReference, CancellationToken cancellationToken = default);
Task<Transfer?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
Task<List<Transfer>> GetByCustomerIdAsync(CustomerId customerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
void Update(Transfer transfer);
```

#### Database Schema

**Transfers Table:**
```sql
CREATE TABLE Transfers (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  TransferReference NVARCHAR(50) NOT NULL UNIQUE,
  IdempotencyKey NVARCHAR(100) NULL UNIQUE,

  SourceAccountId UNIQUEIDENTIFIER NOT NULL,
  SourceCustomerId UNIQUEIDENTIFIER NOT NULL,

  DestinationAccountId UNIQUEIDENTIFIER NOT NULL,
  DestinationCustomerId UNIQUEIDENTIFIER NOT NULL,
  DestinationUserTag NVARCHAR(50) NULL,

  Amount DECIMAL(18,2) NOT NULL,
  AmountCurrency NVARCHAR(3) NOT NULL,
  Fee DECIMAL(18,2) NULL,
  FeeCurrency NVARCHAR(3) NULL,
  TotalDebitAmount DECIMAL(18,2) NOT NULL,
  TotalDebitCurrency NVARCHAR(3) NOT NULL,

  Type INT NOT NULL,  -- 1=Internal, 2=External
  Status INT NOT NULL,  -- 1=Pending, 2=Processing, 3=Completed, 4=Failed
  Description NVARCHAR(500) NULL,
  Narration NVARCHAR(200) NULL,
  FailureReason NVARCHAR(500) NULL,

  CreatedAt DATETIME2 NOT NULL,
  CompletedAt DATETIME2 NULL,

  INDEX IX_Transfers_SourceCustomerId,
  INDEX IX_Transfers_DestinationCustomerId,
  INDEX IX_Transfers_CreatedAt
);
```

**Transactions Table:**
```sql
CREATE TABLE Transactions (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  TransactionReference NVARCHAR(50) NOT NULL UNIQUE,
  AccountId UNIQUEIDENTIFIER NOT NULL,
  CustomerId UNIQUEIDENTIFIER NOT NULL,

  Direction INT NOT NULL,  -- 1=Debit, 2=Credit
  Amount DECIMAL(18,2) NOT NULL,
  Currency NVARCHAR(3) NOT NULL,
  BalanceBefore DECIMAL(18,2) NOT NULL,
  BalanceAfter DECIMAL(18,2) NOT NULL,

  TransferId UNIQUEIDENTIFIER NULL,  -- Links to Transfer
  Narration NVARCHAR(200) NOT NULL,
  CreatedAt DATETIME2 NOT NULL,

  INDEX IX_Transactions_AccountId,
  INDEX IX_Transactions_CustomerId,
  INDEX IX_Transactions_CreatedAt
);
```

---

## How It Works

### Transfer Lifecycle

```
1. CREATE
   ↓
   Status: Pending
   Event: TransferInitiatedEvent
   ↓
2. MARK AS PROCESSING
   ↓
   Status: Processing
   ↓
3a. COMPLETE                    3b. FAIL
    ↓                               ↓
    Status: Completed               Status: Failed
    Event: TransferCompletedEvent   Event: TransferFailedEvent
    CompletedAt: timestamp          FailureReason: "reason"
```

### Event Flow

1. **Transfer Created** → Raises `TransferInitiatedEvent`
   - Event Handler should validate accounts exist
   - Check source account has sufficient balance
   - Move to Processing status

2. **Transfer Processing** → Business logic executes
   - Create DEBIT transaction on source account
   - Create CREDIT transaction on destination account
   - Update account balances
   - Complete or Fail the transfer

3. **Transfer Completed** → Raises `TransferCompletedEvent`
   - Event Handler logs completion
   - Can notify users via SignalR/push notifications

4. **Transfer Failed** → Raises `TransferFailedEvent`
   - Event Handler logs failure
   - Can notify user of failure reason

---

## How to Implement & Test

### Step 1: Create Transfer Command (CQRS)

Create folder: `AfriPay.APP/Transfers/Commands/InitiateTransfer/`

**InitiateTransferCommand.cs:**
```csharp
using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Transfers.Commands.InitiateTransfer;

public record InitiateTransferCommand(
    Guid SourceAccountId,
    Guid SourceCustomerId,
    Guid DestinationAccountId,
    Guid DestinationCustomerId,
    string? DestinationUserTag,
    decimal Amount,
    string Currency,
    decimal? FeeAmount,
    int TransferType,  // 1=Internal, 2=External
    string? Description,
    string? IdempotencyKey
) : IRequest<Result<Guid>>;  // Returns TransferId
```

**InitiateTransferCommandHandler.cs:**
```csharp
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
        _logger.LogInformation("Initiating transfer from {SourceAccountId} to {DestinationAccountId}",
            request.SourceAccountId, request.DestinationAccountId);

        // 1. Check for duplicate (idempotency)
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _unitOfWork.Transfers.GetByIdempotencyKeyAsync(
                request.IdempotencyKey, cancellationToken);

            if (existing != null)
            {
                _logger.LogWarning("Duplicate transfer request detected: {IdempotencyKey}",
                    request.IdempotencyKey);
                return Result<Guid>.Success(existing.Id.Value);
            }
        }

        // 2. Validate accounts exist
        var sourceAccount = await _unitOfWork.Accounts.GetByIdAsync(
            AccountId.Create(request.SourceAccountId), cancellationToken);

        if (sourceAccount == null)
            return Result<Guid>.Failure("Source account not found");

        var destinationAccount = await _unitOfWork.Accounts.GetByIdAsync(
            AccountId.Create(request.DestinationAccountId), cancellationToken);

        if (destinationAccount == null)
            return Result<Guid>.Failure("Destination account not found");

        // 3. Check sufficient balance
        var money = new Money(request.Amount, request.Currency);
        var fee = request.FeeAmount.HasValue
            ? new Money(request.FeeAmount.Value, request.Currency)
            : null;

        var totalDebit = fee != null
            ? new Money(money.Amount + fee.Amount)
            : money;

        if (sourceAccount.Balance < totalDebit.Amount)
            return Result<Guid>.Failure("Insufficient balance");

        // 4. Create transfer
        var transfer = Transfer.Create(
            AccountId.Create(request.SourceAccountId),
            CustomerId.Create(request.SourceCustomerId),
            AccountId.Create(request.DestinationAccountId),
            CustomerId.Create(request.DestinationCustomerId),
            money,
            fee,
            (TransferType)request.TransferType,
            request.Description,
            request.DestinationUserTag,
            request.IdempotencyKey
        );

        await _unitOfWork.Transfers.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);  // This dispatches TransferInitiatedEvent

        _logger.LogInformation("Transfer initiated: {TransferId}", transfer.Id.Value);

        return Result<Guid>.Success(transfer.Id.Value);
    }
}
```

**InitiateTransferCommandValidator.cs:**
```csharp
using FluentValidation;

namespace AfriPay.APP.Transfers.Commands.InitiateTransfer;

public class InitiateTransferCommandValidator : AbstractValidator<InitiateTransferCommand>
{
    public InitiateTransferCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account is required");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account is required")
            .NotEqual(x => x.SourceAccountId).WithMessage("Cannot transfer to same account");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Amount exceeds maximum limit");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., NGN, USD)");

        RuleFor(x => x.TransferType)
            .InclusiveBetween(1, 2).WithMessage("Invalid transfer type");
    }
}
```

### Step 2: Create Event Handlers

Create folder: `AfriPay.APP/EventHandlers/`

**TransferInitiatedEventHandler.cs:**
```csharp
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
        _logger.LogInformation("Transfer initiated event received: {TransferId}", notification.TransferId.Value);

        // Get the transfer
        var transfer = await _unitOfWork.Transfers.GetByIdAsync(notification.TransferId, cancellationToken);
        if (transfer == null)
        {
            _logger.LogError("Transfer not found: {TransferId}", notification.TransferId.Value);
            return;
        }

        try
        {
            // Mark as processing
            transfer.MarkAsProcessing();
            _unitOfWork.Transfers.Update(transfer);

            // TODO: Additional business logic here
            // - Lock accounts for processing
            // - Perform additional validations
            // - Call external services for external transfers

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Transfer marked as processing: {TransferId}", notification.TransferId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer: {TransferId}", notification.TransferId.Value);
            transfer.Fail(ex.Message);
            _unitOfWork.Transfers.Update(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
```

**TransferCompletedEventHandler.cs:**
```csharp
using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class TransferCompletedEventHandler : INotificationHandler<TransferCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransferCompletedEventHandler> _logger;

    public TransferCompletedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<TransferCompletedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TransferCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transfer completed event received: {TransferId}", notification.TransferId.Value);

        try
        {
            // Get accounts
            var sourceAccount = await _unitOfWork.Accounts.GetByIdAsync(
                notification.SourceAccountId, cancellationToken);

            var destinationAccount = await _unitOfWork.Accounts.GetByIdAsync(
                notification.DestinationAccountId, cancellationToken);

            if (sourceAccount == null || destinationAccount == null)
            {
                _logger.LogError("Account not found for transfer: {TransferId}", notification.TransferId.Value);
                return;
            }

            // Create DEBIT transaction on source account
            var sourceBalanceBefore = sourceAccount.Balance;
            var debitTransaction = Transaction.CreateDebit(
                notification.SourceAccountId,
                notification.SourceCustomerId,
                notification.TotalDebitAmount,
                sourceBalanceBefore,
                $"Transfer to account",
                notification.TransferId
            );
            await _unitOfWork.Transactions.AddAsync(debitTransaction, cancellationToken);

            // Update source account balance
            sourceAccount.UpdateBalance(sourceAccount.Balance - notification.TotalDebitAmount.Amount);
            _unitOfWork.Accounts.Update(sourceAccount);

            // Create CREDIT transaction on destination account
            var destBalanceBefore = destinationAccount.Balance;
            var creditTransaction = Transaction.CreateCredit(
                notification.DestinationAccountId,
                notification.DestinationCustomerId,
                notification.Amount,
                destBalanceBefore,
                $"Transfer from account",
                notification.TransferId
            );
            await _unitOfWork.Transactions.AddAsync(creditTransaction, cancellationToken);

            // Update destination account balance
            destinationAccount.UpdateBalance(destinationAccount.Balance + notification.Amount.Amount);
            _unitOfWork.Accounts.Update(destinationAccount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Transactions created for transfer: {TransferId}", notification.TransferId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transactions for transfer: {TransferId}",
                notification.TransferId.Value);
            throw;
        }
    }
}
```

### Step 3: Create API Controller

Create file: `AfriPay.API/Controllers/TransferController.cs`

```csharp
using AfriPay.APP.Transfers.Commands.InitiateTransfer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransferController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferController> _logger;

    public TransferController(IMediator mediator, ILogger<TransferController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a new transfer between accounts
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> InitiateTransfer(
        [FromBody] InitiateTransferRequest request,
        CancellationToken cancellationToken)
    {
        var command = new InitiateTransferCommand(
            request.SourceAccountId,
            request.SourceCustomerId,
            request.DestinationAccountId,
            request.DestinationCustomerId,
            request.DestinationUserTag,
            request.Amount,
            request.Currency ?? "NGN",
            request.FeeAmount,
            request.TransferType,
            request.Description,
            request.IdempotencyKey
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new {
            transferId = result.Value,
            message = "Transfer initiated successfully"
        });
    }
}

public record InitiateTransferRequest(
    Guid SourceAccountId,
    Guid SourceCustomerId,
    Guid DestinationAccountId,
    Guid DestinationCustomerId,
    string? DestinationUserTag,
    decimal Amount,
    string? Currency,
    decimal? FeeAmount,
    int TransferType,
    string? Description,
    string? IdempotencyKey
);
```

### Step 4: Register Dependencies

In `AfriPay.APP/DependencyInjection.cs`, ensure MediatR scans the handlers:

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

In `AfriPay.DAL/DependencyInjection.cs`, ensure TransferRepository is registered:

```csharp
services.AddScoped<ITransferRepository, TransferRepository>();
```

### Step 5: Update IUnitOfWork

In `AfriPay.CORE/Interfaces/IUnitOfWork.cs`, add:

```csharp
public interface IUnitOfWork
{
    // ... existing repositories
    ITransferRepository Transfers { get; }
    ITransactionRepository Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

In `AfriPay.DAL/Repositories/UnitOfWork.cs`, implement:

```csharp
public ITransferRepository Transfers { get; }
public ITransactionRepository Transactions { get; }

public UnitOfWork(AfriPayDbContext context)
{
    _context = context;
    // ... existing repositories
    Transfers = new TransferRepository(context);
    Transactions = new TransactionRepository(context);
}
```

---

## Testing Guide

### 1. Test with Postman/Thunder Client

**Create a Transfer:**

```http
POST http://localhost:5000/api/v1/transfer
Content-Type: application/json

{
  "sourceAccountId": "{{sourceAccountGuid}}",
  "sourceCustomerId": "{{sourceCustomerGuid}}",
  "destinationAccountId": "{{destAccountGuid}}",
  "destinationCustomerId": "{{destCustomerGuid}}",
  "destinationUserTag": "@john",
  "amount": 1000.00,
  "currency": "NGN",
  "feeAmount": 10.50,
  "transferType": 1,
  "description": "Test transfer",
  "idempotencyKey": "test-123-unique"
}
```

**Expected Response:**
```json
{
  "transferId": "a1b2c3d4-...",
  "message": "Transfer initiated successfully"
}
```

### 2. Verify in Database

```sql
-- Check transfer was created
SELECT * FROM Transfers ORDER BY CreatedAt DESC;

-- Check transactions were created
SELECT * FROM Transactions WHERE TransferId = 'your-transfer-id';

-- Check account balances updated
SELECT Id, Balance FROM Accounts WHERE Id IN ('source-id', 'dest-id');
```

### 3. Test Error Scenarios

**Insufficient Balance:**
```json
{
  "amount": 999999999.00,
  ...
}
```
Expected: `400 Bad Request` - "Insufficient balance"

**Same Account Transfer:**
```json
{
  "sourceAccountId": "same-guid",
  "destinationAccountId": "same-guid",
  ...
}
```
Expected: `400 Bad Request` - "Cannot transfer to same account"

**Duplicate Idempotency Key:**
Send same request twice with same `idempotencyKey`.
Expected: Second request returns same transferId (no duplicate created)

### 4. Check Logs

The system logs extensively:
```
[INF] Initiating transfer from {SourceAccountId} to {DestinationAccountId}
[INF] Transfer initiated: {TransferId}
[INF] Transfer initiated event received: {TransferId}
[INF] Transfer marked as processing: {TransferId}
[INF] Transfer completed event received: {TransferId}
[INF] Transactions created for transfer: {TransferId}
```

---

## Key Features

### 1. Idempotency
Use `idempotencyKey` to prevent duplicate transfers. If the same key is used twice, the second request returns the existing transfer ID.

### 2. Domain Events
Transfers raise events at key lifecycle points, allowing decoupled event handlers to perform side effects.

### 3. Money Value Object
The `Money` type ensures amounts are always paired with currency, preventing currency mismatch bugs.

### 4. Audit Trail
Every transfer creates two Transaction records (debit + credit), providing a complete audit trail.

### 5. Reference Numbers
Each transfer gets a unique reference: `TXN-20251203-A1B2C3D4`

### 6. Fee Handling
Optional fees are added to the total debit amount, transparently showing users the full cost.

---

## Next Steps

1. **Implement the commands and handlers** as shown above
2. **Create query endpoints** (GetTransferById, GetTransfersByCustomerId)
3. **Add validation** for business rules (daily limits, blacklists, etc.)
4. **Implement external transfer** integration with payment gateways
5. **Add notifications** via SignalR for real-time transfer updates
6. **Add tests** - unit tests for domain logic, integration tests for flows

---

## Common Issues & Solutions

### Issue: Domain events not firing
**Solution:** Ensure AfriPayDbContext.SaveChangesAsync properly handles `AggregateRoot<TransferId>` entities (already fixed in this implementation).

### Issue: Value object not mapping
**Solution:** Ensure value converters are configured in TransferConfiguration and the value object is ignored in DbContext.OnModelCreating.

### Issue: Transactions not created
**Solution:** Check TransferCompletedEventHandler is registered with MediatR and INotificationHandler interface.

### Issue: Balance not updating
**Solution:** Verify Account entity has UpdateBalance method and repository Update is called before SaveChangesAsync.

---

## Summary

✅ **Transfer System is Ready for Implementation**

The foundation is complete:
- Domain model designed
- Database migrated
- Repository pattern implemented
- Events defined
- DbContext configured

Next: Follow the implementation steps above to create commands, handlers, and API endpoints. The system will then be fully operational for handling money transfers between accounts.
