using System;

namespace SecureTransparentDataExchange.Models.Crypto
{
    public sealed class CipherDto
    {
        public byte[] Iv { get; init; } = Array.Empty<byte>();
        public byte[] Tag { get; init; } = Array.Empty<byte>();
        public byte[] Cipher { get; init; } = Array.Empty<byte>();
    }
}
