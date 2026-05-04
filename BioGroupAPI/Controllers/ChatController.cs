using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<ChatController> _logger;
    private static readonly HttpClient _http = new();

    public ChatController(IConfiguration config, ILogger<ChatController> logger)
    {
        _config = config;
        _logger = logger;
    }

    public record ChatMessage(string role, string content);
    public record ChatRequest(List<ChatMessage> messages, string? productsContext);

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return StatusCode(503, new { message = "AI chưa được cấu hình." });

        var systemPrompt = BuildSystemPrompt(req.productsContext);

        var body = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 1024,
            system = systemPrompt,
            messages = req.messages
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error: {json}", json);
                return StatusCode(502, new { message = "AI tạm thời không phản hồi, vui lòng thử lại." });
            }

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return Ok(new { reply = text });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat error");
            return StatusCode(500, new { message = "Lỗi kết nối AI." });
        }
    }

    private static string BuildSystemPrompt(string? productsJson)
    {
        var productSection = string.IsNullOrEmpty(productsJson)
            ? ""
            : $"\n\nDanh sách sản phẩm hiện tại (JSON):\n{productsJson}";

        return $"""
            Bạn là trợ lý tư vấn của BioWraps Vietnam — công ty chuyên cung cấp bao bì sinh học từ vỏ cam,
            thân thiện với môi trường, an toàn thực phẩm và tự phân hủy 100%.

            Nhiệm vụ:
            - Tư vấn sản phẩm phù hợp với nhu cầu khách hàng
            - Giải thích ưu điểm của bao bì sinh học
            - Trả lời câu hỏi về giá, tồn kho, khuyến mãi dựa trên dữ liệu sản phẩm được cung cấp
            - Hướng dẫn đặt hàng nếu khách hỏi

            Quy tắc:
            - Luôn trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp
            - Giữ câu trả lời ngắn gọn (tối đa 3-4 câu) trừ khi khách cần chi tiết
            - Không bịa đặt thông tin sản phẩm nếu không có trong danh sách
            - Nếu không biết, đề nghị khách liên hệ hotline hoặc để lại thông tin{productSection}
            """;
    }
}
