using AfriPay.APP.Common.Models;
using AfriPay.CORE.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<Result<LoginResponse>>
    {
        /// <summary>
        /// Customer email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (Bearer)
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public CustomerId CustomerId { get; set; }

        /// <summary>
        /// Customer email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer full name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Account number (if available)
        /// </summary>
        //public AccountId AccountId { get; set; }

        /// <summary>
        /// Timestamp when token was issued
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Timestamp when token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
