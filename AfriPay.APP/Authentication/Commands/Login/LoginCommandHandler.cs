using AfriPay.APP.Common.Models;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Authentication.Commands.Login
{
    /// <summary>
    /// Handler for LoginCommand
    /// Authenticates customer and generates JWT token
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            ICustomerRepository customerRepository,
            IAccountRepository accountRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<LoginCommandHandler> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Login attempt for UserTag: {UserTag}",
                request.UserTag);

            // Find customer by UserTag
            var customer = await _customerRepository.GetByUserTagAsync(request.UserTag, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning(
                    "Login failed - customer not found: {UserTag}",
                    request.UserTag);

                return Result<LoginResponse>.Failure(
                    "Invalid usertag or password");
            }

            // Check if customer has activated their account (has password set)
            if (string.IsNullOrEmpty(customer.PasswordHash))
            {
                _logger.LogWarning(
                    "Login failed - account not activated: {CustomerId}",
                    customer.CustomerId);

                return Result<LoginResponse>.Failure(
                    "Account not activated. Please set your UserTag and password first.");
            }

            // Check if customer is active
            if (!customer.IsActive)
            {
                _logger.LogWarning(
                    "Login failed - customer account is inactive: {CustomerId}",
                    customer.CustomerId);

                return Result<LoginResponse>.Failure(
                    "Account is inactive. Please contact support.");
            }

            // Verify password (compares against hash set during UserTag creation)
            var isPasswordValid = _passwordHasher.VerifyPassword(
                request.Password,
                customer.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogWarning(
                    "Login failed - invalid password for customer: {CustomerId}",
                    customer.CustomerId);

                return Result<LoginResponse>.Failure(
                    "Invalid usertag or password");
            }

            // Get customer's account (if exists)
            var account = await _accountRepository.GetByCustomerIdAsync(
                customer.CustomerId, // Use .Value to get the Guid
                cancellationToken);

            // Generate JWT token
            var tokenExpirationMinutes = 60; // 1 hour
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(tokenExpirationMinutes);

            var token = _jwtTokenGenerator.GenerateToken(
                customer.CustomerId, // Use .Value to get the Guid
                customer.ContactInfo!.Email,
                customer.FirstName,
                tokenExpirationMinutes);

            _logger.LogInformation(
                "Login successful for customer: {CustomerId}",
                customer.CustomerId);

            // Build response
            var response = new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = tokenExpirationMinutes * 60, // Convert to seconds
                CustomerId = customer.CustomerId, // Use .Value to get the Guid
                Email = customer.ContactInfo.Email,
                FirstName = customer.FirstName,

                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };

            return Result<LoginResponse>.Success(response);
        }
    }
}