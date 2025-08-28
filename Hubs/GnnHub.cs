using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

// SignalR Hub sınıfı: Client ile LLM arasındaki köprü
public class GnnHub : Hub
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Her bağlantı için CancellationToken tutmak için sözlük
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsMap = new();

    public GnnHub(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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
    public async Task Recommend(int user_id, int k)
    {

        // Bu istemci için iptal token oluştur ve sözlüğe ekle
        var cts = new CancellationTokenSource();
        _ctsMap[Context.ConnectionId] = cts;

        try
        {
            // HttpClient oluştur
            var http = _httpClientFactory.CreateClient("GnnClient");
            http.Timeout = Timeout.InfiniteTimeSpan; // stream sonsuz akabileceği için timeout yok


            var payload = new
            {
                user_id = user_id,
                k = k
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            // POST isteği hazırlıyoruz
            var req = new HttpRequestMessage(HttpMethod.Post, "http://9.223.178.203:5002/recommend")
            {
                Content = content
            };

            // İsteği gönder stream olarak cevap al
            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            resp.EnsureSuccessStatusCode();

            // Response body'yi stream olarak aç
            await using var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            // Gnn'den gelen satırları tek tek oku
            while ((line = await reader.ReadLineAsync()) is not null && !cts.Token.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    // Satır JSON ise parse et
                    using var doc = JsonDocument.Parse(line);

                    if (doc.RootElement.TryGetProperty("response", out var respText))
                    {
                        var token = respText.GetString();
                        if (!string.IsNullOrEmpty(token))
                            await Clients.Caller.SendAsync("token", token);
                    }

                    if (doc.RootElement.TryGetProperty("done", out var done) && done.GetBoolean())
                        break;
                }
                catch
                {
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
