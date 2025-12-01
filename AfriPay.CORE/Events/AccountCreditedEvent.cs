using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events
{
    /// <summary>
    /// Event raised when an account is credited
    /// </summary>
    public class AccountCreditedEvent : DomainEvent
    {
        public AccountId AccountId { get; }
        public Money Amount { get; }
        public string TransactionReference { get; }
        public string? Description { get; }

        public AccountCreditedEvent(
            AccountId accountId,
            Money amount,
            string transactionReference,
            string? description = null)
        {
            AccountId = accountId;
            Amount = amount;
            TransactionReference = transactionReference;
            Description = description;
        }
    }
}
