using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities
{
    public class Account : AggregateRoot<AccountId>
    {
        public AccountId AccountId { get; private set; }
        public AccountNumber AccountNumber { get; private set; }
        public AccountType AccountType { get; private set; }
        public Money Balance { get; private set; }
        public Money ReservedBalance { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public Customer Customer { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
        public DateTime DateOpened { get; private set; }
        public bool IsActive { get; private set; }
        public string? ProviderReference { get; private set; } // Reference from VA Provider
        public PrimaryAccountInfo? PrimaryAccountInfo { get; private set; } // For PAPS integration

        // Transaction limits
        public decimal DailyTransferLimit { get; private set; }
        public decimal SingleTransferLimit { get; private set; }
        public decimal DailyTransferTotal { get; private set; }
        public DateTime? LastDailyResetDate { get; private set; }

        private Account() { } // EF Core

        private Account(
            AccountNumber accountNumber,
            AccountType accountType,
            CustomerId customerId,
            string? providerReference = null)
        {
            AccountId = AccountId.Create();
            AccountNumber = accountNumber;
            AccountType = accountType;
            CustomerId = customerId;
            Balance = new Money(0);
            ReservedBalance = new Money(0);
            DateOpened = DateTime.UtcNow;
            IsActive = true;
            ProviderReference = providerReference;

            // Default limits (can be configured per account type)
            DailyTransferLimit = 1000000; // 1M
            SingleTransferLimit = 500000;  // 500K
            DailyTransferTotal = 0;
            LastDailyResetDate = DateTime.UtcNow.Date;
        }

        public static Account CreateVirtualAccount(
            CustomerId customerId,
            AccountNumber accountNumber,
            string providerReference)
        {
            var account = new Account(
                accountNumber: accountNumber,
                accountType: AccountType.Virtual,
                customerId: customerId,
                providerReference: providerReference
            );

            account.AddDomainEvent(new VirtualAccountCreatedEvent(
                account.AccountId,
                account.AccountNumber,
                account.CustomerId,
                providerReference
            ));

            return account;
        }

        public Result Credit(Money amount, string transactionReference, string? description = null)
        {
            if (!IsActive)
                return Result.Failure("Account is not active");

            if (amount.Amount <= 0)
                return Result.Failure("Credit amount must be positive");

            Balance += amount;

            AddDomainEvent(new AccountCreditedEvent(
                AccountId,
                amount,
                transactionReference,
                description));

            return Result.Success();
        }

        public Result Debit(Money amount, string transactionReference, string? description = null)
        {
            if (!IsActive)
                return Result.Failure("Account is not active");

            if (amount.Amount <= 0)
                return Result.Failure("Debit amount must be positive");

            ResetDailyLimitIfNeeded();

            // Check single transfer limit
            if (amount.Amount > SingleTransferLimit)
                return Result.Failure($"Amount exceeds single transfer limit of {SingleTransferLimit}");

            // Check daily limit
            if (DailyTransferTotal + amount.Amount > DailyTransferLimit)
                return Result.Failure($"Amount exceeds daily transfer limit of {DailyTransferLimit}");

            // Check available balance (balance - reserved)
            var availableBalance = Balance.Amount - ReservedBalance.Amount;
            if (amount.Amount > availableBalance)
                return Result.Failure("Insufficient available balance");

            Balance -= amount;
            DailyTransferTotal += amount.Amount;

            AddDomainEvent(new AccountDebitedEvent(
                AccountId,
                amount,
                transactionReference,
                description));

            return Result.Success();
        }

        public Result ReserveFunds(Money amount, string reservationReference)
        {
            if (!IsActive)
                return Result.Failure("Account is not active");

            if (amount.Amount <= 0)
                return Result.Failure("Reservation amount must be positive");

            var availableBalance = Balance.Amount - ReservedBalance.Amount;
            if (amount.Amount > availableBalance)
                return Result.Failure("Insufficient available balance for reservation");

            ReservedBalance += amount;

            return Result.Success();
        }

        public Result ReleaseFunds(Money amount)
        {
            if (amount.Amount <= 0)
                return Result.Failure("Release amount must be positive");

            if (amount.Amount > ReservedBalance.Amount)
                return Result.Failure("Release amount exceeds reserved balance");

            ReservedBalance -= amount;

            return Result.Success();
        }

        public void SetPrimaryAccountInfo(PrimaryAccountInfo primaryAccountInfo)
        {
            PrimaryAccountInfo = primaryAccountInfo ?? throw new ArgumentNullException(nameof(primaryAccountInfo));
        }

        public void UpdateTransferLimits(decimal dailyLimit, decimal singleLimit)
        {
            if (dailyLimit <= 0 || singleLimit <= 0)
                throw new ArgumentException("Limits must be positive");

            if (singleLimit > dailyLimit)
                throw new ArgumentException("Single transfer limit cannot exceed daily limit");

            DailyTransferLimit = dailyLimit;
            SingleTransferLimit = singleLimit;
        }

        private void ResetDailyLimitIfNeeded()
        {
            var today = DateTime.UtcNow.Date;
            if (LastDailyResetDate < today)
            {
                DailyTransferTotal = 0;
                LastDailyResetDate = today;
            }
        }

        public decimal GetAvailableBalance()
        {
            return Balance.Amount - ReservedBalance.Amount;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Suspend()
        {
            IsActive = false;
        }

        public void Close()
        {
            if (Balance.Amount > 0)
                throw new InvalidOperationException("Cannot close account with positive balance");

            IsActive = false;
        }
    }
}
