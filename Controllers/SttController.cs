using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using RagBasedChatbot.Helpers;

namespace RagBasedChatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/stt
    public class SttController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _sttUrl;

        public SttController(IHttpClientFactory f, IConfiguration cfg)
        {
            _httpFactory = f;
            _sttUrl = cfg["ExternalServices:SttUrl"]
                      ?? throw new InvalidOperationException("ExternalServices:SttUrl missing");

            var ff = cfg["Ffmpeg:Path"];
            if (!string.IsNullOrWhiteSpace(ff)) RagBasedChatbot.Helpers.FfmpegConvert.OverridePath = ff;
        }

        [HttpPost, RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Post([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0) return BadRequest("Empty audio file");

            // 1) RAM'e al
            await using var src = file.OpenReadStream();
            var buf = new MemoryStream();
            await src.CopyToAsync(buf, ct);
            buf.Position = 0;

            // 2) WebM/OGG -> WAV (16k mono). Önce pipe, olmazsa temp fallback
            MemoryStream wav;
            try
            {
                wav = await FfmpegConvert.ToWav16kMonoAsync(buf, ct);
            }
            catch
            {
                buf.Position = 0;
                wav = await FfmpegConvert.ToWav16kMonoViaTempAsync(buf, ".webm", ct);
            }

            // 3) WAV'ı uzak STT'ye gönder
            var http = _httpFactory.CreateClient();
            using var form = new MultipartFormDataContent();
            var sc = new StreamContent(wav);
            sc.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            form.Add(sc, "file", "speech.wav");

            using var resp = await http.PostAsync(_sttUrl, form, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, body);

            // 4) {text:"..."} veya düz metin
            string? text = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("text", out var t)) text = t.GetString();
                else if (doc.RootElement.TryGetProperty("transcript", out var tr)) text = tr.GetString();
            }
            catch { text = body; }

            return Ok(new { text = string.IsNullOrWhiteSpace(text) ? "[BLANK_AUDIO]" : text });
        }

        [HttpGet("ping")] public IActionResult Ping() => Ok("pong");
    }
}
