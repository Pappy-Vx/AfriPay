using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Onboarding.Commands.StartOnboarding
{
    /// <summary>
    /// Handler for StartOnboardingCommand
    /// Initiates the onboarding process and publishes domain events
    /// </summary>
    public class StartOnboardingCommandHandler : IRequestHandler<StartOnboardingCommand, Result<StartOnboardingResponse>>
    {
        private readonly IOnboardingRequestRepository _onboardingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<StartOnboardingCommandHandler> _logger;

        public StartOnboardingCommandHandler(
            IOnboardingRequestRepository onboardingRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IEventPublisher eventPublisher,
            ILogger<StartOnboardingCommandHandler> logger)
        {
            _onboardingRepository = onboardingRepository ?? throw new ArgumentNullException(nameof(onboardingRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<StartOnboardingResponse>> Handle(
            StartOnboardingCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Starting onboarding process for {Email}",
                    request.Email);

                // Check for duplicate email
                var existingCustomerByEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingCustomerByEmail != null)
                {
                    _logger.LogWarning(
                        "Onboarding failed: Email {Email} already exists",
                        request.Email);
                    return Result.Failure<StartOnboardingResponse>("A customer with this email already exists");
                }

                // Check for duplicate BVN
                var bvn = new BVN(request.BVN);
                var existingCustomerByBvn = await _customerRepository.GetByBvnAsync(bvn, cancellationToken);
                if (existingCustomerByBvn != null)
                {
                    _logger.LogWarning(
                        "Onboarding failed: BVN already registered");
                    return Result.Failure<StartOnboardingResponse>("A customer with this BVN already exists");
                }

                // Check for duplicate onboarding request by BVN
                var existingOnboardingByBvn = await _onboardingRepository.GetByBvnAsync(request.BVN, cancellationToken);
                if (existingOnboardingByBvn != null)
                {
                    _logger.LogWarning(
                        "Onboarding failed: BVN already has a pending onboarding request");
                    return Result.Failure<StartOnboardingResponse>("An onboarding request with this BVN already exists");
                }

                // Create onboarding request
                var onboardingRequest = OnboardingRequest.Create(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.BVN);

                // Save to repository
                await _onboardingRepository.AddAsync(onboardingRequest, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Onboarding request created: {OnboardingId}",
                    onboardingRequest.OnboardingId);

                // Publish domain events
                await _eventPublisher.PublishManyAsync(onboardingRequest.DomainEvents, cancellationToken);

                // Return response
                var response = new StartOnboardingResponse
                {
                    OnboardingId = onboardingRequest.OnboardingId,
                    RequestReference = onboardingRequest.RequestReference,
                    Status = onboardingRequest.Status.ToString(),
                    Message = "Onboarding request created successfully. BVN verification in progress.",
                    RequestedAt = onboardingRequest.RequestedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error starting onboarding for {Email}",
                    request.Email);
                return Result.Failure<StartOnboardingResponse>("An error occurred while processing the onboarding request");
            }
        }
    }
}
