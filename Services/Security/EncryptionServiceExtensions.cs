using SecureTransparentDataExchange.Models.Crypto;
using SecureTransparentDataExchange.Services;
using System;
using System.Threading.Tasks;

namespace SecureTransparentDataExchange.Services.Security
{
    public static class EncryptionServiceExtensions
    {
        public static async Task<(string Base64, CipherDto Parts)> EncryptToCipherDtoAsync(
            this EncryptionService svc, string userId, string plainText)
        {
            var b64 = await svc.EncryptAsync(plainText);
            var bytes = Convert.FromBase64String(b64);

            var iv = new byte[12];
            var tag = new byte[16];
            var cipher = new byte[bytes.Length - iv.Length - tag.Length];

            Buffer.BlockCopy(bytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(bytes, iv.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(bytes, iv.Length + tag.Length, cipher, 0, cipher.Length);

            return (b64, new CipherDto { Iv = iv, Tag = tag, Cipher = cipher });
        }

        public static Task<string> DecryptFromCipherDtoAsync(
            this EncryptionService svc, string userId, CipherDto c)
        {
            var buf = new byte[c.Iv.Length + c.Tag.Length + c.Cipher.Length];

            Buffer.BlockCopy(c.Iv, 0, buf, 0, c.Iv.Length);
            Buffer.BlockCopy(c.Tag, 0, buf, c.Iv.Length, c.Tag.Length);
            Buffer.BlockCopy(c.Cipher, 0, buf, c.Iv.Length + c.Tag.Length, c.Cipher.Length);

            return svc.DecryptAsync(Convert.ToBase64String(buf));
        }
    }
}