using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
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

        // Personal Information
        public PersonalInfo PersonalInfo { get; private set; }
        public string FirstName => PersonalInfo.FirstName;
        public string LastName => PersonalInfo.LastName;
        public string? MiddleName => PersonalInfo.MiddleName;
        public DateTime DateOfBirth => PersonalInfo.DateOfBirth;

        // Contact Information
        public ContactInfo ContactInfo { get; private set; }
        public string Email => ContactInfo.Email;
        public string PhoneNumber => ContactInfo.PhoneNumber;

        // Identity
        public IdentityNumber IdentityNumber { get; private set; }
        public Country Country { get; private set; }
        public string SelfieUrl { get; private set; }

        public OnboardingStatus Status { get; private set; }
        public CustomerId? CustomerId { get; private set; }
        public Customer? Customer { get; private set; }
        public AccountId? VirtualAccountId { get; private set; }
        public Account? VirtualAccount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? FailureReason { get; private set; }

        // Legacy support - keep for backward compatibility
        public BVN? BVN => IdentityNumber is BVN bvn ? bvn : null;
        public DateTime RequestedAt => CreatedAt;

        private OnboardingRequest() { } // EF Core

        private OnboardingRequest(
            PersonalInfo personalInfo,
            ContactInfo contactInfo,
            IdentityNumber identityNumber,
            Country country,
            string selfieUrl)
        {
            OnboardingId = Guid.NewGuid();
            Id = OnboardingId; // Set base AggregateRoot.Id for event tracking
            RequestReference = $"ONB-{OnboardingId:N}".ToUpper();
            PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
            IdentityNumber = identityNumber ?? throw new ArgumentNullException(nameof(identityNumber));
            Country = country;
            SelfieUrl = selfieUrl ?? throw new ArgumentNullException(nameof(selfieUrl));
            Status = OnboardingStatus.Initiated;
            CreatedAt = DateTime.UtcNow;
        }

        public static OnboardingRequest Create(
            PersonalInfo personalInfo,
            ContactInfo contactInfo,
            IdentityNumber identityNumber,
            Country country,
            string selfieUrl)
        {
            var request = new OnboardingRequest(
                personalInfo,
                contactInfo,
                identityNumber,
                country,
                selfieUrl);

            request.AddDomainEvent(new OnboardingRequestedEvent(
                request.OnboardingId,
                request.RequestReference,
                request.FirstName,
                request.LastName,
                request.IdentityNumber
            ));

            return request;
        }

        /// <summary>
        /// Legacy Create method for backward compatibility - use Create(PersonalInfo, ContactInfo, IdentityNumber, Country, string) instead
        /// </summary>
        [Obsolete("Use Create(PersonalInfo, ContactInfo, IdentityNumber, Country, string) instead")]
        public static OnboardingRequest Create(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string bvnValue)
        {
            var personalInfo = new PersonalInfo(firstName, lastName, null, DateTime.UtcNow.AddYears(-25));
            var contactInfo = new ContactInfo(email, phoneNumber);
            var bvn = BVN.Create(bvnValue);

            return Create(personalInfo, contactInfo, bvn, Country.Nigeria, "https://placeholder.com/selfie.jpg");
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
