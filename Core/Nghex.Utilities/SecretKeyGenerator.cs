using System.Security.Cryptography;

namespace Nghex.Utilities
{
    public class SecretKeyGenerator
    {
        private static readonly char[] _allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        /// <summary>
        /// Create a random secret key
        /// </summary>
        /// <param name="length">Length of the secret key</param>
        /// <returns>Random secret key</returns>
        public static string CreateRandomSecretKey(int length = 64)
        {
            try
            {
                var keyLength = length <= 16 ? 16 : length;
                var keyChars = new char[keyLength];
                var randomBytes = new byte[keyLength];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                for (int i = 0; i < keyLength; i++)
                    keyChars[i] = _allowedChars[randomBytes[i] % _allowedChars.Length];
                return new string(keyChars);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating random secret key", ex);
            }
        }
    }
}