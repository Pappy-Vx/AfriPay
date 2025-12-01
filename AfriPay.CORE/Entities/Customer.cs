using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities
{
    public class Customer : AggregateRoot<CustomerId>
    {
        public CustomerId CustomerId { get; private set; }
        public CustomerReference CustomerReference { get; private set; }

        // Backward compatibility - kept for existing code
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public BVN BVN { get; private set; }

        // Enhanced value objects (optional for future use)
        public PersonalInfo? PersonalInfo { get; private set; }
        public ContactInfo? ContactInfo { get; private set; }
        public Address? Address { get; private set; }

        public bool IsBvnVerified { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public CustomerStatus Status { get; private set; }

        // Navigation properties
        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

        private readonly List<OnboardingRequest> _onboardingRequests = new();
        public IReadOnlyCollection<OnboardingRequest> OnboardingRequests => _onboardingRequests.AsReadOnly();

        private Customer() { } // EF Core

        private Customer(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            BVN bvn)
        {
            CustomerId = CustomerId.Create();
            CustomerReference = CustomerReference.Create();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            BVN = bvn;
            IsBvnVerified = false;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Status = CustomerStatus.PendingActivation;
        }

        public static Customer Create(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            BVN bvn)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required", nameof(phoneNumber));

            var customer = new Customer(firstName, lastName, email, phoneNumber, bvn);

            customer.AddDomainEvent(new CustomerCreatedEvent(
                customer.CustomerId,
                customer.CustomerReference,
                customer.FirstName,
                customer.LastName,
                customer.Email
            ));

            return customer;
        }

        public void VerifyBvn()
        {
            if (IsBvnVerified)
                throw new InvalidOperationException("BVN already verified");

            IsBvnVerified = true;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new BvnVerifiedEvent(CustomerId, BVN));
        }

        public void AddAccount(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            _accounts.Add(account);
        }

        public void AddOnboardingRequest(OnboardingRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _onboardingRequests.Add(request);
        }

        public void Activate()
        {
            Status = CustomerStatus.Active;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend(string reason)
        {
            Status = CustomerStatus.Suspended;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Close(string reason)
        {
            Status = CustomerStatus.Closed;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePersonalInfo(PersonalInfo personalInfo)
        {
            PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateContactInfo(ContactInfo contactInfo)
        {
            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
            Email = contactInfo.Email;
            PhoneNumber = contactInfo.PhoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAddress(Address address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
