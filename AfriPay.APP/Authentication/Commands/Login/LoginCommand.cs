using AfriPay.APP.Common.Models;
using AfriPay.CORE.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Authentication.Commands.Login
{
    /// <summary>
    /// Login request containing customer credentials
    /// </summary>
    public class LoginCommand : IRequest<Result<LoginResponse>>
    {
        /// <summary>
        /// Customer usertag address (must be valid email format)
        /// </summary>
        /// <example>John</example>
        [Required(ErrorMessage = "UserTag is required")]
        [MinLength(3,ErrorMessage = "UserTag must be at least 3 Characters")]
        public string UserTag { get; set; } = string.Empty;

        /// <summary>
        /// Customer password (minimum 8 characters)
        /// </summary>
        /// <example>SecurePassword123!</example>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Successful login response containing JWT token and customer information
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT access token for authenticated API requests
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c</example>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (always "Bearer" for JWT tokens)
        /// </summary>
        /// <example>Bearer</example>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Token expiration time in seconds (typically 3600 = 1 hour)
        /// </summary>
        /// <example>3600</example>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Unique customer identifier (GUID)
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public CustomerId CustomerId { get; set; }

        /// <summary>
        /// Customer's registered email address
        /// </summary>
        /// <example>john.doe@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer's first name
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Account number (if available)
        /// </summary>
        //public AccountId AccountId { get; set; }

        /// <summary>
        /// UTC timestamp when the token was issued
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the token will expire
        /// </summary>
        /// <example>2024-01-15T11:30:00Z</example>
        public DateTime ExpiresAt { get; set; }
    }
}
