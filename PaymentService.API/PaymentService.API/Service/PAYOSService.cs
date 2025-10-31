using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentService.API.Models.DTO;
using PaymentService.API.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.API.Service
{
    public class PayOSService : IPayOSService
    {
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;

        public PayOSService(IConfiguration config)
        {
            _baseUrl = config["PayOS:BaseUrl"];
            _clientId = config["PayOS:ClientId"];
            _apiKey = config["PayOS:ApiKey"];
            _checksumKey = config["PayOS:ChecksumKey"];
            _returnUrl = config["PayOS:ReturnUrl"];
            _cancelUrl = config["PayOS:CancelUrl"];

            // ✅ Kiểm tra cấu hình có null không
            if (string.IsNullOrEmpty(_baseUrl))
                throw new Exception("❌ PayOS BaseUrl is missing in appsettings.json");
            if (string.IsNullOrEmpty(_clientId))
                throw new Exception("❌ PayOS ClientId is missing in appsettings.json");
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("❌ PayOS ApiKey is missing in appsettings.json");
            if (string.IsNullOrEmpty(_checksumKey))
                throw new Exception("❌ PayOS ChecksumKey is missing in appsettings.json");
            if (string.IsNullOrEmpty(_returnUrl))
                throw new Exception("❌ PayOS ReturnUrl is missing in appsettings.json");
            if (string.IsNullOrEmpty(_cancelUrl))
                throw new Exception("❌ PayOS CancelUrl is missing in appsettings.json");
        }

        public async Task<string> CreatePaymentLink(long orderCode, long amount, string description)
        {
            string endpoint = "/v2/payment-requests";
            var url = $"{_baseUrl.TrimEnd('/')}{endpoint}";

            var sorted = new SortedDictionary<string, object>
            {
                { "amount", amount },
                { "cancelUrl", _cancelUrl },
                { "description", description },
                { "orderCode", orderCode },
                { "returnUrl", _returnUrl }
            };

            string raw = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
            string signature = ComputeHmacSha256(_checksumKey, raw);

            var body = new PaymentRequest
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                returnUrl = _returnUrl,
                cancelUrl = _cancelUrl,
                signature = signature
            };

            string json = JsonConvert.SerializeObject(body);

            // 🧾 Ghi log chi tiết
            Console.WriteLine("---- PayOS Request ----");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Raw: {raw}");
            Console.WriteLine($"Signature: {signature}");
            Console.WriteLine($"Body: {json}");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            var response = await client.SendAsync(request);
            var respJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine("---- PayOS Response ----");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine(respJson);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayOS API error: {respJson}");

            var respObj = JsonConvert.DeserializeObject<PaymentResponse>(respJson)
                          ?? throw new Exception($"Invalid PayOS response: {respJson}");

            if (respObj.data == null || string.IsNullOrEmpty(respObj.data.checkoutUrl))
                throw new Exception($"PayOS API error: {respObj.code} - {respObj.desc}");

            return respObj.data.checkoutUrl;
        }

        private string ComputeHmacSha256(string key, string data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Checksum key cannot be null or empty.", nameof(key));
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
