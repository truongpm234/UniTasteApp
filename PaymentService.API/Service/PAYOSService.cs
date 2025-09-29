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
    public class PayOSService
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
            // 🔹 Log thông tin gửi đi
            Console.WriteLine("---- PayOS Request ----");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Raw String: {raw}");
            Console.WriteLine($"Signature: {signature}");
            Console.WriteLine($"Request JSON: {json}");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-client-id", _clientId);
            request.Headers.Add("x-api-key", _apiKey);

            var response = await client.SendAsync(request);
            var respJson = await response.Content.ReadAsStringAsync();
            // 🔹 Log thông tin nhận về
            Console.WriteLine("---- PayOS Response ----");
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine($"Response JSON: {respJson}");

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
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
