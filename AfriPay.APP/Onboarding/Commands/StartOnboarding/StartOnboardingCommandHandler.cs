using AfriPay.APP.Common.Models;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
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
        private readonly IOnboardingRequestRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StartOnboardingCommandHandler> _logger;

        public StartOnboardingCommandHandler(
            IOnboardingRequestRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<StartOnboardingCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<StartOnboardingResponse>> Handle(
            StartOnboardingCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting onboarding for {Email}, {PhoneNumber}",
                request.Email,
                request.PhoneNumber);

            // Check for duplicate
            var exists = await _repository.ExistsAsync(
                request.Email,
                request.PhoneNumber,
                cancellationToken);

            if (exists)
            {
                _logger.LogWarning(
                    "Duplicate onboarding attempt for {Email}",
                    request.Email);

                return Result<StartOnboardingResponse>.Failure(
                    "An onboarding request already exists for this email or phone number");
            }

            // Create PersonalInfo value object
            var personalInfo = new PersonalInfo(
                request.FirstName,
                request.LastName,
                request.MiddleName,
                request.DateOfBirth);

            // Create ContactInfo value object
            var contactInfo = new ContactInfo(
                request.Email,
                request.PhoneNumber);

            // Create IdentityNumber based on type
            IdentityNumber identityNumber = request.IdentityType switch
            {
                IdentityType.BVN => BVN.Create(request.IdentityNumber),
                IdentityType.GhanaCard => GhanaCard.Create(request.IdentityNumber),
                IdentityType.KenyaNationalID => KenyaNationalID.Create(request.IdentityNumber),
                _ => throw new ArgumentException($"Unsupported identity type: {request.IdentityType}")
            };

            // Create OnboardingRequest aggregate
            var onboardingRequest = OnboardingRequest.Create(
                personalInfo,
                contactInfo,
                identityNumber,
                request.Country,
                request.SelfieUrl);

            // Save
            await _repository.AddAsync(onboardingRequest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Onboarding request created with ID {OnboardingId}",
                onboardingRequest.OnboardingId);

            // Return response
            var response = new StartOnboardingResponse
            {
                RequestId = onboardingRequest.RequestReference,
                OnboardingId = onboardingRequest.OnboardingId,
                Status = onboardingRequest.Status,
                CreatedAt = onboardingRequest.CreatedAt
            };

            return Result<StartOnboardingResponse>.Success(response);
        }
    }
}
