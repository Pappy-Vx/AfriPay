using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hash a password using secure algorithm
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify a password against its hash
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);
    }
}
