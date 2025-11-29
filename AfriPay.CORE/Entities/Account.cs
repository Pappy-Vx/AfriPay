using AfriPay.CORE.Common;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Entities
{
    public class Account : AggregateRoot<AccountId>
    {
        public AccountId AccountId { get; private set; }
        public AccountNumber AccountNumber { get; private set; }
        //public AccountType AccountType { get; private set; }
        public Money Balance { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public Customer Customer { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
        public DateTime DateOpened { get; private set; }
        public bool IsActive { get; private set; }
        public string? ProviderReference { get; private set; } // Reference from VA Provider

        private Account() { } // EF Core

        private Account(
            AccountNumber accountNumber,
            //AccountType accountType,
            CustomerId customerId,
            string? providerReference = null)
        {
            AccountId = AccountId.Create();
            AccountNumber = accountNumber;
            //AccountType = accountType;
            CustomerId = customerId;
            Balance = new Money(0);
            DateOpened = DateTime.UtcNow;
            IsActive = true;
            ProviderReference = providerReference;
        }

        public static Account CreateVirtualAccount(
            CustomerId customerId,
            AccountNumber accountNumber,
            string providerReference)
        {
            var account = new Account(
                accountNumber: accountNumber,
                //accountType: AccountType.Virtual,
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

        public Result Credit(Money amount, string description)
        {
            if (!IsActive)
                return Result.Failure("Account is not active");

            if (amount.Amount <= 0)
                return Result.Failure("Credit amount must be positive");

            Balance += amount;

            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }

}
