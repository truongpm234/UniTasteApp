using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PaymentService.API.Service;
using System.Text;
using System.Text.Json;

namespace PaymentService.API.Controller
{
    [ApiController]
    [Route("api/webhooks/payos")]
    [AllowAnonymous]
    public class WebhooksController : ControllerBase
    {
        private readonly IConfiguration _cfg;
        private readonly IPaymentService _payment;
        private readonly ILogger<WebhooksController> _log;

        public WebhooksController(IConfiguration cfg, IPaymentService payment, ILogger<WebhooksController> log)
        {
            _cfg = cfg;
            _payment = payment;
            _log = log;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Handle([FromBody] JsonDocument payload)
        {
            // 1) Lấy checksum key
            var checksum = _cfg["PayOS:ChecksumKey"];
            if (string.IsNullOrWhiteSpace(checksum))
                return StatusCode(500, "Missing PayOS:ChecksumKey");

            // 2) Lấy signature & vùng dữ liệu cần ký
            string? signature = null;
            JsonElement dataElem;

            var root = payload.RootElement;

            // Case A: kiểu payOS mới (code/desc/success + data + signature)
            if (root.TryGetProperty("data", out var dataInBody) &&
                root.TryGetProperty("signature", out var sigInBody) &&
                sigInBody.ValueKind == JsonValueKind.String)
            {
                dataElem = dataInBody;
                signature = sigInBody.GetString();
            }
            // Case B: kiểu “phẳng” (như mẫu bạn gửi khi tạo link) — signature nằm ở root
            else if (root.TryGetProperty("signature", out var sigFlat) &&
                     sigFlat.ValueKind == JsonValueKind.String)
            {
                signature = sigFlat.GetString();
                // ký trên toàn bộ root TRỪ khóa signature
                dataElem = root;
            }
            else
            {
                _log.LogWarning("Webhook missing signature");
                return BadRequest("Invalid payload: signature not found");
            }

            // 3) Tạo dataString (sort key, serialize object/array con) và verify
            var dataString = (dataElem.ValueKind == JsonValueKind.Object && ReferenceEquals(dataElem, root))
                ? BuildDataStringFromRootExcludingSignature(root)    // phẳng
                : BuildDataString(dataElem);                         // có 'data'

            var expected = HmacHex(checksum!, dataString);
            if (!string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase))
            {
                _log.LogWarning("Signature verification FAILED");
                return Unauthorized("Invalid signature");
            }

            // 4) Trích các trường cần dùng (từ data nếu có, nếu không từ root)
            var src = root.TryGetProperty("data", out var d) ? d : root;

            string? paymentLinkId = TryGetString(src, "paymentLinkId");
            long amount = TryGetInt64(src, "amount");
            long orderCode = TryGetInt64(src, "orderCode");

            // status ưu tiên; nếu không có thì suy theo code/desc
            string status = (TryGetString(src, "status") ?? TryGetString(root, "status") ?? "")
                            .Trim().ToUpperInvariant();
            var code = (TryGetString(src, "code") ?? TryGetString(root, "code") ?? "").Trim();

            if (string.IsNullOrEmpty(status))
            {
                status = code == "00" ? "PAID" : "FAILED"; // mặc định theo code
            }

            // 5) Cập nhật trạng thái (an toàn idempotent phía service)
            switch (status)
            {
                case "PAID":
                    if (!string.IsNullOrWhiteSpace(paymentLinkId))
                        _payment.MarkPaidAsync(paymentLinkId!, amount).GetAwaiter().GetResult();
                    break;

                case "CANCELED":
                case "CANCELLED":
                    if (!string.IsNullOrWhiteSpace(paymentLinkId))
                        _payment.MarkCanceledAsync(paymentLinkId!).GetAwaiter().GetResult();
                    break;

                case "FAILED":
                default:
                    if (!string.IsNullOrWhiteSpace(paymentLinkId))
                        _payment.MarkFailedAsync(paymentLinkId!).GetAwaiter().GetResult();
                    break;
            }

            return Ok(new { ok = true });
        }

        // ===== Helpers =====

        // Ký theo 'data' (object) — sort key, object/array con được serialize sau khi sort
        private static string BuildDataString(JsonElement obj)
        {
            var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var p in obj.EnumerateObject())
            {
                map[p.Name] = p.Value.ValueKind switch
                {
                    JsonValueKind.Null => "",
                    JsonValueKind.Object => SerializeSortedObject(p.Value),
                    JsonValueKind.Array => SerializeArrayWithSortedObjects(p.Value),
                    _ => p.Value.ToString() ?? ""
                };
            }
            return string.Join("&", map.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        // Ký trên root nhưng BỎ QUA 'signature'
        private static string BuildDataStringFromRootExcludingSignature(JsonElement root)
        {
            var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var p in root.EnumerateObject())
            {
                if (string.Equals(p.Name, "signature", StringComparison.Ordinal)) continue;

                map[p.Name] = p.Value.ValueKind switch
                {
                    JsonValueKind.Null => "",
                    JsonValueKind.Object => SerializeSortedObject(p.Value),
                    JsonValueKind.Array => SerializeArrayWithSortedObjects(p.Value),
                    _ => p.Value.ToString() ?? ""
                };
            }
            return string.Join("&", map.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        private static string SerializeSortedObject(JsonElement o)
        {
            var inner = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            foreach (var p in o.EnumerateObject())
            {
                inner[p.Name] = p.Value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Object => JsonSerializer.Deserialize<object>(SerializeSortedObject(p.Value))!,
                    JsonValueKind.Array => JsonSerializer.Deserialize<object>(SerializeArrayWithSortedObjects(p.Value))!,
                    _ => JsonSerializer.Deserialize<object>(p.Value.GetRawText())!
                };
            }
            return JsonSerializer.Serialize(inner);
        }

        private static string SerializeArrayWithSortedObjects(JsonElement arr)
        {
            var list = new List<object?>();
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(item.ValueKind == JsonValueKind.Object
                    ? JsonSerializer.Deserialize<object>(SerializeSortedObject(item))!
                    : JsonSerializer.Deserialize<object>(item.GetRawText())!);
            }
            return JsonSerializer.Serialize(list);
        }

        private static string HmacHex(string secret, string data)
        {
            using var h = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
        }

        private static string? TryGetString(JsonElement elem, string name)
            => elem.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static long TryGetInt64(JsonElement elem, string name)
            => elem.TryGetProperty(name, out var v) && v.TryGetInt64(out var i) ? i : 0;
    }
}
