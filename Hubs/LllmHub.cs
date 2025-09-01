using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

// SignalR Hub sınıfı: Client ile LLM arasındaki köprü
public class LlmHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LlmOptions _opt;

    // Her bağlantı için CancellationToken tutmak için sözlük
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsMap = new();

    public LlmHub(IHttpClientFactory httpClientFactory, IOptions<LlmOptions> opt)
    {
        _httpClientFactory = httpClientFactory;
        _opt = opt.Value;
    }

    // Bir kullanıcı bağlantıyı kapatınca çalışan kısım
    // Eğer yarım kalmış istek varsa iptal ediliyor
    // Bu fonksiyonu henüz devreye sokmadım 
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_ctsMap.TryRemove(Context.ConnectionId, out var cts))
            cts.Cancel();
        return base.OnDisconnectedAsync(exception);
    }

    // Kullanıcı istemciden "Generate" çağırınca çalışıyor
    // Prompt'u alıyor ve LLM'e post atıyor
    public async Task Generate(string prompt, bool? useAzure = null)
    {
        // Kullanıcı "useAzure" parametresi verdiyse onu kullan,
        // yoksa appsettings.json'daki varsayılan değeri al
        var baseUrl = (useAzure ?? _opt.UseAzure) ? _opt.AzureBaseUrl : _opt.LocalBaseUrl;

        // Eğer URL boşsa hata dön
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            await Clients.Caller.SendAsync("error", "LLM base URL missing.");
            return;
        }

        // Bu istemci için iptal token oluştur ve sözlüğe ekle
        var cts = new CancellationTokenSource();
        _ctsMap[Context.ConnectionId] = cts;

        try
        {
            // HttpClient oluştur
            var http = _httpClientFactory.CreateClient("LlmClient");
            http.Timeout = Timeout.InfiniteTimeSpan; // stream sonsuz akabileceği için timeout yok

        



            // POST isteği hazırlıyoruz
            var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/v1/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    model = "phi3:mini",  // kullanılacak model adı
                    prompt = prompt,    // kullanıcının gönderdiği prompt
                    stream = true       // cevap akış halinde gelsin
                }), Encoding.UTF8, "application/json")
            };

            // İsteği gönder stream olarak cevap al
            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            resp.EnsureSuccessStatusCode();

            // Response body'yi stream olarak aç
            await using var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            var fullResponse = new StringBuilder();
            // LLM'den gelen satırları tek tek oku
            while ((line = await reader.ReadLineAsync()) is not null && !cts.Token.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    // Satır JSON ise parse et
                    using var doc = JsonDocument.Parse(line);

                    // Ollama gibi JSON formatında "response" alanı döner
                    if (doc.RootElement.TryGetProperty("response", out var respText))
                    {
                        var token = respText.GetString();
                        if (!string.IsNullOrEmpty(token))
                            // Token'i anında client'a gönder
                            await Clients.Caller.SendAsync("token", token);
                    }

                    // "done": true geldiyse stream bitmiştir -> döngüden çık
                    if (doc.RootElement.TryGetProperty("done", out var done) && done.GetBoolean())
                        break;
                }
                catch
                {
                    fullResponse.Append(line);
                    // JSON parse edilemezse düz metin olarak gönder
                    await Clients.Caller.SendAsync("token", line);
                }
            }

            // İstemciye işin bittiğini bildir
            await Clients.Caller.SendAsync("completed");
        }
        catch (OperationCanceledException)
        {
            // Kullanıcı Cancel çağırdıysa
            await Clients.Caller.SendAsync("completed", "canceled");
        }
        catch (Exception ex)
        {
            // Beklenmedik hata olursa
            await Clients.Caller.SendAsync("error", ex.Message);
        }
        finally
        {
            // Kaynağı temizle
            _ctsMap.TryRemove(Context.ConnectionId, out _);
            cts.Dispose();
        }
    }

    // İstemci bu metodu çağırırsa → iptal işlemi tetiklenir
    public Task Cancel()
    {
        if (_ctsMap.TryRemove(Context.ConnectionId, out var cts))
            cts.Cancel();
        return Task.CompletedTask;
    }
}

// appsettings.json'daki ayarları tutan sınıf
public sealed class LlmOptions
{
    public string? LocalBaseUrl { get; set; }
    public string? AzureBaseUrl { get; set; }
    public bool UseAzure { get; set; } = false;
}
