using AfriPay.CORE.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Services
{
    public class  PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8; // 16 bytes
        private const int HashSize = 256 / 8; // 32 bytes
        private const int Iterations = 100000; // OWASP recommendation

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // Generate salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash password
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            // Combine salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrEmpty(passwordHash))
                return false;

            try
            {
                // Decode the hash
                byte[] hashBytes = Convert.FromBase64String(passwordHash);

                // Ensure hash is correct length
                if (hashBytes.Length != SaltSize + HashSize)
                    return false;

                // Extract salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Extract stored hash
                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

                // Hash the provided password with extracted salt
                byte[] computedHash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: Iterations,
                    numBytesRequested: HashSize);

                // Compare hashes using constant-time comparison
                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
