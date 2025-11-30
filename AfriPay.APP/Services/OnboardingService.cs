using AfriPay.APP.DTOs;
using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBvnVerificationService _bvnService;
        private readonly IVirtualAccountProvider _vaProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OnboardingService> _logger;

        public OnboardingService(
            IUnitOfWork unitOfWork,
            IBvnVerificationService bvnService,
            IVirtualAccountProvider vaProvider,
            IEventPublisher eventPublisher,
            ILogger<OnboardingService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _bvnService = bvnService ?? throw new ArgumentNullException(nameof(bvnService));
            _vaProvider = vaProvider ?? throw new ArgumentNullException(nameof(vaProvider));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<OnboardingResponse>> StartOnboardingAsync(
            OnboardingStartRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting onboarding for {Email}", request.Email);

                // Check if customer already exists
                var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
                if (existingCustomer != null)
                {
                    return Result.Failure<OnboardingResponse>("Customer with this email already exists");
                }

                var existingByBvn = await _unitOfWork.Customers.GetByBvnAsync(request.BVN, cancellationToken);
                if (existingByBvn != null)
                {
                    return Result.Failure<OnboardingResponse>("Customer with this BVN already exists");
                }

                // Create onboarding request
                var onboardingRequest = OnboardingRequest.Create(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.BVN
                );

                await _unitOfWork.OnboardingRequests.AddAsync(onboardingRequest, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish domain events
                await _eventPublisher.PublishManyAsync(onboardingRequest.DomainEvents, cancellationToken);
                onboardingRequest.ClearDomainEvents();

                _logger.LogInformation("Onboarding request created: {RequestReference}", onboardingRequest.RequestReference);

                return Result.Success(new OnboardingResponse
                {
                    OnboardingId = onboardingRequest.OnboardingId,
                    RequestReference = onboardingRequest.RequestReference,
                    Status = onboardingRequest.Status.ToString(),
                    Message = "Onboarding request created successfully. BVN verification will be processed.",
                    RequestedAt = onboardingRequest.RequestedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting onboarding for {Email}", request.Email);
                return Result.Failure<OnboardingResponse>($"Error starting onboarding: {ex.Message}");
            }
        }

        public async Task<Result<OnboardingResponse>> GetOnboardingStatusAsync(
            Guid onboardingId,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(onboardingId, cancellationToken);
            if (request == null)
            {
                return Result.Failure<OnboardingResponse>("Onboarding request not found");
            }

            return Result.Success(new OnboardingResponse
            {
                OnboardingId = request.OnboardingId,
                RequestReference = request.RequestReference,
                Status = request.Status.ToString(),
                Message = GetStatusMessage(request),
                CustomerReference = request.Customer?.CustomerReference.Value,
                AccountNumber = request.VirtualAccount?.AccountNumber.Value,
                RequestedAt = request.RequestedAt,
                CompletedAt = request.CompletedAt
            });
        }

        private string GetStatusMessage(OnboardingRequest request)
        {
            return request.Status switch
            {
                CORE.Enums.OnboardingStatus.Initiated => "Onboarding initiated",
                CORE.Enums.OnboardingStatus.BvnVerificationPending => "BVN verification in progress",
                CORE.Enums.OnboardingStatus.BvnVerified => "BVN verified successfully",
                CORE.Enums.OnboardingStatus.BvnVerificationFailed => $"BVN verification failed: {request.FailureReason}",
                CORE.Enums.OnboardingStatus.CustomerCreated => "Customer created",
                CORE.Enums.OnboardingStatus.VirtualAccountCreationPending => "Virtual account creation in progress",
                CORE.Enums.OnboardingStatus.VirtualAccountCreated => "Virtual account created",
                CORE.Enums.OnboardingStatus.VirtualAccountCreationFailed => $"Virtual account creation failed: {request.FailureReason}",
                CORE.Enums.OnboardingStatus.Completed => "Onboarding completed successfully",
                CORE.Enums.OnboardingStatus.Failed => $"Onboarding failed: {request.FailureReason}",
                _ => "Unknown status"
            };
        }
    }
}
