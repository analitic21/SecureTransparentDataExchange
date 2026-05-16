using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace SecureTransparentDataExchange.Models
{
    public class EncryptionKey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PublicKey { get; set; } = string.Empty;
        
        [Required]
        public string PrivateKey { get; set; } = string.Empty;
        public ICollection<GeoFenceZone> GeoFenceZones { get; set; } = new List<GeoFenceZone>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public ICollection<JwtSetting> JwtSettings { get; set; } = new List<JwtSetting>();
        public bool IsValid()
        {
            return ExpiresAt == null || ExpiresAt > DateTime.UtcNow;
        }

        public void RotateKeys()
        {
            using var rsa = RSA.Create(2048);
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddYears(1);
        }

        // =========================
        // ⚠️ LEGACY (for compatibility)
        // =========================
        [Obsolete("Use EncryptionService instead. This method is kept for compatibility.")]
        public static string EncryptKey(string plainText, string _)
        {
            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("AcademicStaticKey"));
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Convert.ToBase64String(aes.IV.Concat(cipher).ToArray());
        }

        [Obsolete("Use EncryptionService instead. This method is kept for compatibility.")]
        public static string DecryptKey(string encryptedText, string _)
        {
            var raw = Convert.FromBase64String(encryptedText);
            var iv = raw[..16];
            var cipher = raw[16..];

            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes("AcademicStaticKey"));
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plain);
        }
    }
}
