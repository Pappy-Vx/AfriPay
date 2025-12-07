using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;

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

        // Multi-country identity support
        public string IdentityNumber { get; private set; } // Stores the actual identity number (BVN, Ghana Card, Kenya ID, etc.)
        public IdentityType IdentityType { get; private set; } // Enum: BVN, GhanaCard, KenyaNationalID, etc.
        public bool IsIdentityVerified { get; private set; }

        // Deprecated - kept for backward compatibility
        //[Obsolete("Use IdentityNumber and IdentityType instead")]
        //public BVN? BVN { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        public UserTag? UserTag { get; private set; }
        public DateTime? UserTagSetAt { get; private set; }

        // Enhanced value objects (optional for future use)
        public PersonalInfo? PersonalInfo { get; private set; }
        public ContactInfo? ContactInfo { get; private set; }
        public Address? Address { get; private set; }

        [Obsolete("Use IsIdentityVerified instead")]
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
            //string passwordHash,
            string identityNumber,
            IdentityType identityType)
        {
            CustomerId = CustomerId.Create();
            Id = CustomerId; // Set base AggregateRoot.Id for event tracking
            CustomerReference = CustomerReference.Create();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            //PasswordHash = passwordHash;
            IdentityNumber = identityNumber;
            IdentityType = identityType;
            UserTag = null; // Will be set later by user
            UserTagSetAt = null;
            IsIdentityVerified = false;
            IsBvnVerified = false; // Kept for backward compatibility
            //BVN = null; // Deprecated field
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Status = CustomerStatus.PendingActivation;
        }

        /// <summary>
        /// Creates a new customer with identity verification
        /// Supports multiple identity types: BVN (Nigeria), Ghana Card, Kenya National ID, etc.
        /// </summary>
        public static Customer Create(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            //string passwordHash,
            IdentityNumber identity)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            //if (string.IsNullOrWhiteSpace(passwordHash))
            //    throw new ArgumentException("Password is required", nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required", nameof(phoneNumber));
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            // Determine identity type from the IdentityNumber value object
            var identityType = identity switch
            {
                BVN => IdentityType.BVN,
                GhanaCard => IdentityType.GhanaCard,
                KenyaNationalID => IdentityType.KenyaNationalID,
                _ => throw new ArgumentException($"Unsupported identity type: {identity.GetType().Name}")
            };

            var customer = new Customer(
                firstName,
                lastName,
                email,
                phoneNumber,
                //passwordHash,
                identity.Value,
                identityType);

            //            // Backward compatibility: Set BVN if identity is BVN
            //            if (identity is BVN bvn)
            //            {
            //#pragma warning disable CS0618 // Type or member is obsolete
            //                customer.BVN = bvn;
            //#pragma warning restore CS0618
            //            }

            // Initialize ContactInfo to sync with flat properties
            customer.UpdateContactInfo(new ContactInfo(email, phoneNumber));

            customer.AddDomainEvent(new CustomerCreatedEvent(
                customer.CustomerId,
                customer.CustomerReference,
                customer.FirstName,
                customer.LastName,
                customer.ContactInfo!.Email
            ));

            return customer;
        }

        //    public static Customer Create(
        //string firstName,
        //string lastName,
        //string email,
        //string phoneNumber,
        //string passwordHash,
        //GhanaCard ghanaCard)
        //    {
        //        // implementation...
        //    }

        //public static Customer Create(
        //    string firstName,
        //    string lastName,
        //    string email,
        //    string phoneNumber,
        //    string passwordHash,
        //    BVN bvn)
        //{
        //    // Validation
        //    if (string.IsNullOrWhiteSpace(firstName))
        //        throw new ArgumentException("First name is required", nameof(firstName));

        //    if (string.IsNullOrWhiteSpace(lastName))
        //        throw new ArgumentException("Last name is required", nameof(lastName));

        //    if (string.IsNullOrWhiteSpace(email))
        //        throw new ArgumentException("Email is required", nameof(email));
        //    if(string.IsNullOrWhiteSpace(passwordHash))
        //        throw new ArgumentException("Password", nameof(passwordHash));

        //    if (string.IsNullOrWhiteSpace(phoneNumber))
        //        throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        //    var customer = new Customer(firstName, lastName, email, phoneNumber,passwordHash, bvn);

        //    customer.AddDomainEvent(new CustomerCreatedEvent(
        //        customer.CustomerId,
        //        customer.CustomerReference,
        //        customer.FirstName,
        //        customer.LastName,
        //        customer.ContactInfo.Email

        //    ));

        //    return customer;
        //}

        //public void VerifyBvn()
        //{
        //    if (IsBvnVerified)
        //        throw new InvalidOperationException("BVN already verified");

        //    IsBvnVerified = true;
        //    UpdatedAt = DateTime.UtcNow;

        //    AddDomainEvent(new BvnVerifiedEvent(CustomerId, BVN));
        //}

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

        // SetUserTag: Set UserTag for the first time, or update within 7 days
        public Result SetUserTag(UserTag userTag)
        {
            if (userTag == null)
                return Result.Failure("UserTag cannot be null.");

            // If UserTag already set, only allow change within 7 days
            if (UserTag != null && DateTime.UtcNow > CreatedAt.AddDays(7))
                return Result.Failure("UserTag can only be changed within 7 days of account creation.");

            UserTag = userTag;
            UserTagSetAt = DateTime.UtcNow;

            AddDomainEvent(new UserTagSetEvent(CustomerId, userTag));

            return Result.Success();
        }

        /// <summary>
        /// Marks the customer's primary identity document as verified.
        /// For backward compatibility, this also updates IsBvnVerified when the identity type is BVN.
        /// </summary>
        public void MarkIdentityVerified()
        {
            if (IsIdentityVerified)
                throw new InvalidOperationException("Identity already verified");

            IsIdentityVerified = true;

            // Preserve legacy BVN flag semantics for existing consumers
            if (IdentityType == IdentityType.BVN)
            {
                IsBvnVerified = true;
            }

            UpdatedAt = DateTime.UtcNow;
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

        /// </summary>
        public void SetPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

            PasswordHash = passwordHash;
        }


        /// <summary>
        /// Deactivate the customer account
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
