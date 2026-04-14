using System.Security.Cryptography;
using System.Text;

namespace Nghex.Utilities
{
    /// <summary>
    /// Interface for Password Service
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hash password
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify password
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);

    }

    /// <summary>
    /// Password Service implementation
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Generate salt
            var salt = GenerateSalt();
            var saltBytes = Convert.FromBase64String(salt);

            // Hash password with salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(HashSize);

            // Combine salt and hash
            var combinedBytes = new byte[SaltSize + HashSize];
            Array.Copy(saltBytes, 0, combinedBytes, 0, SaltSize);
            Array.Copy(hashBytes, 0, combinedBytes, SaltSize, HashSize);

            return Convert.ToBase64String(combinedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                // Extract salt and hash from stored password
                var combinedBytes = Convert.FromBase64String(hashedPassword);
                if (combinedBytes.Length != SaltSize + HashSize)
                    return false;

                var saltBytes = new byte[SaltSize];
                var storedHashBytes = new byte[HashSize];
                Array.Copy(combinedBytes, 0, saltBytes, 0, SaltSize);
                Array.Copy(combinedBytes, SaltSize, storedHashBytes, 0, HashSize);

                // Hash the provided password with the same salt
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
                var computedHashBytes = pbkdf2.GetBytes(HashSize);

                // Compare hashes
                return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}

