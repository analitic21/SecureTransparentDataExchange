using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services
{
    public sealed class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(EncryptedKeyService keyService)
        {
            var activeKey = keyService.GetActive();

            if (activeKey == null)
                throw new InvalidOperationException("No active encryption key found.");

            // хэшируем ключ из базы → получаем 256-bit ключ
            _key = SHA256.HashData(
     Encoding.UTF8.GetBytes(activeKey.PrivateKey)
 
            );
        }

        public Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentException("Plain text is required.");

            return Task.FromResult(EncryptInternal(plainText));
        }

        public Task<string> DecryptAsync(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                throw new ArgumentException("Encrypted text is required.");

            return Task.FromResult(DecryptInternal(encryptedText));
        }

        private string EncryptInternal(string plainText)
        {
            var iv = RandomNumberGenerator.GetBytes(12);
            var data = Encoding.UTF8.GetBytes(plainText);
            var cipher = new byte[data.Length];
            var tag = new byte[16];

            using var aes = new AesGcm(_key, 16);
            aes.Encrypt(iv, data, cipher, tag);

            return Convert.ToBase64String(
                iv.Concat(tag).Concat(cipher).ToArray()
            );
        }

        private string DecryptInternal(string encryptedText)
        {
            var raw = Convert.FromBase64String(encryptedText);
            var iv = raw[..12];
            var tag = raw[12..28];
            var cipher = raw[28..];
            var plain = new byte[cipher.Length];

            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(iv, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }
    }
}