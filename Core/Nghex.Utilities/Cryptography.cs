using System.Security.Cryptography;
using System.Text;

namespace Nghex.Utilities
{
    /// <summary>
    /// Interface for configuration key provider
    /// </summary>
    public interface IEncryptionKeyProvider
    {
        /// <summary>
        /// Gets encryption key from configuration
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns>Encryption key value or null if not found</returns>
        string? GetKey(string key);

        /// <summary>
        /// Sets encryption key to configuration
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Encryption key value</param>
        /// <returns>True if successful</returns>
        bool SetKey(string key, string value);
    }

    public class Cryptography
    {
        private readonly string _encryptionKey;
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const int IvSize = 16;
        private const string EncryptionKeyConfigKey = "ENCRYPTION_KEY";

        /// <summary>
        /// Initializes a new instance of the Cryptography class with a default encryption key.
        /// Will try to load key from configuration, or generate a new one if not found.
        /// </summary>
        public Cryptography() : this(GetDefaultKey(null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Cryptography class with encryption key provider.
        /// Will try to load key from configuration, or generate a new one if not found.
        /// </summary>
        /// <param name="keyProvider">The encryption key provider to load encryption key from.</param>
        public Cryptography(IEncryptionKeyProvider? keyProvider) : this(GetDefaultKey(keyProvider))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Cryptography class with a specified encryption key.
        /// </summary>
        /// <param name="encryptionKey">The encryption key. Must be at least 32 characters long.</param>
        /// <exception cref="ArgumentException">Thrown when encryption key is null, empty, or too short.</exception>
        public Cryptography(string encryptionKey)
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptionKey));

            if (encryptionKey.Length < 32)
                throw new ArgumentException("Encryption key must be at least 32 characters long", nameof(encryptionKey));

            _encryptionKey = encryptionKey;
        }

        /// <summary>
        /// Gets a fingerprint of the encryption key for diagnostic purposes.
        /// Returns first 8 chars of SHA256 hash of the key (safe to log).
        /// </summary>
        public string GetKeyFingerprint()
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
            return Convert.ToHexString(hash).Substring(0, 8);
        }

        /// <summary>
        /// Verifies the encryption key by encrypting and decrypting a test value.
        /// Returns true if encryption/decryption works correctly.
        /// </summary>
        public bool VerifyKey()
        {
            try
            {
                const string testValue = "LICENSE_KEY_TEST_2024";
                var encrypted = Encrypt(testValue);
                var decrypted = Decrypt(encrypted);
                return decrypted == testValue;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypts a string value using AES encryption.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>Base64-encoded encrypted string.</returns>
        /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
        public string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));

            try
            {
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Derive key from encryption key
                aes.Key = DeriveKey(_encryptionKey, KeySize / 8);

                // Generate random IV
                aes.GenerateIV();
                var iv = aes.IV;

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(value);
                var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Prepend IV to cipher text
                var result = new byte[IvSize + cipherBytes.Length];
                Array.Copy(iv, 0, result, 0, IvSize);
                Array.Copy(cipherBytes, 0, result, IvSize, cipherBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error encrypting value", ex);
            }
        }

        /// <summary>
        /// Decrypts a Base64-encoded encrypted string.
        /// </summary>
        /// <param name="value">The Base64-encoded encrypted string to decrypt.</param>
        /// <returns>The decrypted string.</returns>
        /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
        /// <exception cref="CryptographicException">Thrown when decryption fails (invalid key, corrupted data, etc.).</exception>
        public string Decrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));

            try
            {
                // Trim whitespace, BOM, and zero-width characters that might cause Base64 parsing issues
                var cleanValue = value.Trim().TrimStart('\uFEFF', '\u200B');
                var fullCipher = Convert.FromBase64String(cleanValue);

                if (fullCipher.Length < IvSize)
                    throw new CryptographicException("Invalid encrypted data format");

                // Extract IV and cipher text
                var iv = new byte[IvSize];
                var cipher = new byte[fullCipher.Length - IvSize];
                Array.Copy(fullCipher, 0, iv, 0, IvSize);
                Array.Copy(fullCipher, IvSize, cipher, 0, cipher.Length);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;
                aes.Key = DeriveKey(_encryptionKey, KeySize / 8);

                using var decryptor = aes.CreateDecryptor();
                var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Invalid Base64 format", ex);
            }
            catch (CryptographicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error decrypting value", ex);
            }
        }

        /// <summary>
        /// Derives a key of the specified size from the encryption key using SHA256.
        /// </summary>
        private static byte[] DeriveKey(string key, int keySize)
        {
            using var sha256 = SHA256.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = sha256.ComputeHash(keyBytes);
            var derivedKey = new byte[keySize];
            Array.Copy(hash, 0, derivedKey, 0, Math.Min(hash.Length, keySize));
            return derivedKey;
        }

        /// <summary>
        /// Gets a default encryption key from configuration or generates a new one.
        /// Nếu config chưa có key thì tạo mới bằng SecretKeyGenerator.CreateRandomSecretKey(64),
        /// ngược lại load key từ config.
        /// </summary>
        /// <param name="keyProvider">The encryption key provider to load encryption key from. Can be null.</param>
        /// <returns>The encryption key from config or a newly generated one.</returns>
        public static string GetDefaultKey(IEncryptionKeyProvider? keyProvider)
        {
            try
            {
                if (keyProvider != null)
                {
                    var configKey = keyProvider.GetKey(EncryptionKeyConfigKey);
                    if (!string.IsNullOrWhiteSpace(configKey) && configKey.Length >= 32)
                        return configKey;
                    var newKey = SecretKeyGenerator.CreateRandomSecretKey(64);
                    _ = keyProvider.SetKey(EncryptionKeyConfigKey, newKey);
                    return newKey;
                }
            }
            catch {}
            return SecretKeyGenerator.CreateRandomSecretKey(64);
        }
    }
}