using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generate a JWT token for authenticated user
        /// </summary>
        string GenerateToken(
            CustomerId customerId,
            string email,
            string fullName,
            int expirationMinutes = 60);
    }
}
