using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RagBasedChatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TtsController : ControllerBase
    {
        // Tek bir HttpClient'ı paylaş (socket sızıntısı olmasın)
        private static readonly HttpClient _http = new HttpClient
        {
            // XTTS CPU/GPU durumuna göre biraz uzun sürebilir
            Timeout = TimeSpan.FromMinutes(2)
        };

        private readonly string _pythonTtsBase;

        public TtsController(IConfiguration cfg)
        {
            // Öncelik: ENV > appsettings > fallback sabit
            _pythonTtsBase =
                Environment.GetEnvironmentVariable("PYTHON_TTS_BASE")
                ?? cfg["PythonTts:BaseUrl"]
                ?? "http://localhost:8009";
        }

        // İstek gövdesi
        public record TtsRequest(
            string text,
            string? language = "en",
            string? speaker_wav = null,
            string? voice = null
        );

        [HttpPost] // POST /api/tts
        public async Task<IActionResult> Post([FromBody] TtsRequest body)
        {
            try
            {
                var text = (body.text ?? string.Empty).Trim();
                var lang = string.IsNullOrWhiteSpace(body.language) ? "en" : body.language!.Trim();

                if (string.IsNullOrWhiteSpace(text))
                    return BadRequest("text is empty.");

                Console.WriteLine($"[TTS] POST → {_pythonTtsBase}/tts | lang={lang} | len={text.Length}");

                var payload = JsonSerializer.Serialize(new
                {
                    text,
                    language = lang,
                    body.speaker_wav,
                    body.voice
                });

                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var resp = await _http.PostAsync($"{_pythonTtsBase.TrimEnd('/')}/tts", content);

                Console.WriteLine($"[TTS] Python status: {(int)resp.StatusCode} {resp.ReasonPhrase}");

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine("[TTS] Backend error -> " + err);
                    return StatusCode((int)resp.StatusCode, err);
                }

                // Bazı servisler doğrudan audio döndürür, bazıları JSON + base64
                var ctype = resp.Content.Headers.ContentType?.MediaType?.ToLowerInvariant() ?? "";

                byte[] wavBytes;

                if (ctype.StartsWith("audio/"))
                {
                    wavBytes = await resp.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    // JSON bekle: { "audio": "<base64>", "mime": "audio/wav" }
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("audio", out var audioEl))
                        return StatusCode(502, "TTS backend: missing 'audio' field.");
                    var b64 = audioEl.GetString() ?? "";
                    wavBytes = Convert.FromBase64String(b64);
                }

                if (wavBytes.Length == 0)
                    return StatusCode(502, "TTS backend: empty audio.");

                Response.Headers["X-TTS-Engine"] = "xtts_v2";
                return File(wavBytes, "audio/wav", "reply.wav");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[TTS] EXCEPTION: " + ex);
                return StatusCode(500, "TTS failed: " + ex.Message);
            }
        }

        [HttpGet("health")] // GET /api/tts/health
        public async Task<IActionResult> Health()
        {
            using var resp = await _http.GetAsync($"{_pythonTtsBase.TrimEnd('/')}/health");
            var text = await resp.Content.ReadAsStringAsync();
            return Content(text, "application/json");
        }
    }
}
