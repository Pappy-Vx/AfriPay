using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Entities
{
    public class OnboardingRequest : AggregateRoot<Guid>
    {
        public Guid OnboardingId { get; private set; }
        public string RequestReference { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public BVN BVN { get; private set; }
        public OnboardingStatus Status { get; private set; }
        public CustomerId? CustomerId { get; private set; }
        public Customer? Customer { get; private set; }
        public AccountId? VirtualAccountId { get; private set; }
        public Account? VirtualAccount { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? FailureReason { get; private set; }

        private OnboardingRequest() { } // EF Core

        private OnboardingRequest(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            BVN bvn)
        {
            OnboardingId = Guid.NewGuid();
            RequestReference = $"ONB-{OnboardingId:N}".ToUpper();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            BVN = bvn;
            Status = OnboardingStatus.Initiated;
            RequestedAt = DateTime.UtcNow;
        }

        public static OnboardingRequest Create(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string bvnValue)
        {
            var bvn = BVN.Create(bvnValue);
            var request = new OnboardingRequest(firstName, lastName, email, phoneNumber, bvn);

            request.AddDomainEvent(new OnboardingRequestedEvent(
                request.OnboardingId,
                request.RequestReference,
                request.BVN,
                request.FirstName,
                request.LastName,
                request.Email
            ));

            return request;
        }

        public void MarkBvnVerificationPending()
        {
            Status = OnboardingStatus.BvnVerificationPending;
        }

        public Result MarkBvnVerified()
        {
            if (Status != OnboardingStatus.BvnVerificationPending)
                return Result.Failure("Invalid status transition");

            Status = OnboardingStatus.BvnVerified;

            AddDomainEvent(new BvnVerifiedForOnboardingEvent(OnboardingId, BVN));

            return Result.Success();
        }

        public Result MarkBvnVerificationFailed(string reason)
        {
            Status = OnboardingStatus.BvnVerificationFailed;
            FailureReason = reason;
            CompletedAt = DateTime.UtcNow;

            return Result.Failure(reason);
        }

        public void LinkCustomer(CustomerId customerId)
        {
            if (Status != OnboardingStatus.BvnVerified)
                throw new InvalidOperationException("Cannot link customer before BVN verification");

            CustomerId = customerId;
            Status = OnboardingStatus.CustomerCreated;
        }

        public void MarkVirtualAccountCreationPending()
        {
            if (Status != OnboardingStatus.CustomerCreated)
                throw new InvalidOperationException("Customer must be created first");

            Status = OnboardingStatus.VirtualAccountCreationPending;
        }

        public Result LinkVirtualAccount(AccountId accountId)
        {
            if (Status != OnboardingStatus.VirtualAccountCreationPending)
                return Result.Failure("Invalid status for virtual account creation");

            VirtualAccountId = accountId;
            Status = OnboardingStatus.VirtualAccountCreated;

            return Result.Success();
        }

        public Result MarkVirtualAccountCreationFailed(string reason)
        {
            Status = OnboardingStatus.VirtualAccountCreationFailed;
            FailureReason = reason;
            CompletedAt = DateTime.UtcNow;

            return Result.Failure(reason);
        }

        public Result Complete()
        {
            if (Status != OnboardingStatus.VirtualAccountCreated)
                return Result.Failure("Cannot complete onboarding - virtual account not created");

            Status = OnboardingStatus.Completed;
            CompletedAt = DateTime.UtcNow;

            AddDomainEvent(new OnboardingCompletedEvent(
                OnboardingId,
                CustomerId!,
                VirtualAccountId!,
                RequestReference
            ));

            return Result.Success();
        }

        public Result Fail(string reason)
        {
            Status = OnboardingStatus.Failed;
            FailureReason = reason;
            CompletedAt = DateTime.UtcNow;

            return Result.Failure(reason);
        }
    }

}
