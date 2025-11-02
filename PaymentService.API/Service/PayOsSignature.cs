using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PaymentService.API.Service
{
    public sealed class PayOsSignature : IPayOsSignature
    {
        private readonly byte[] _secret;
        public PayOsSignature(IConfiguration config)
        {
            var key = config["PayOS:ChecksumKey"]
                      ?? throw new Exception("PayOS:ChecksumKey missing");
            _secret = Encoding.UTF8.GetBytes(key);
        }

        public bool Verify(byte[] body, string signatureHex)
        {
            using var hmac = new HMACSHA256(_secret);
            var hash = hmac.ComputeHash(body); // <- dùng byte[] chuẩn
            var expected = Convert.ToHexString(hash).ToLowerInvariant();

            // chấp nhận cả hex viết hoa/thường
            var provided = signatureHex?.Trim().ToLowerInvariant();

            // Một số hệ thống gửi base64 -> nếu cần, thử thêm decode base64 để so:
            // if (provided?.Contains('.') == false && provided?.Length % 4 == 0) { ... }

            return string.Equals(expected, provided, StringComparison.Ordinal);
        }
    }
}