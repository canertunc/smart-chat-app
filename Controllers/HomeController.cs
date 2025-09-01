using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using RagBasedChatbot.Models;
using RagBasedChatbot.Helpers;

namespace RagBasedChatbot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ScrollToBottom = true;
            // Session
            var messages = HttpContext.Session.GetObject<List<ChatMessage>>("ChatMessages") 
                           ?? new List<ChatMessage>();

            return View(messages);
        }

        [HttpPost("saveMessage")]
        public IActionResult SaveMessage([FromBody] ChatMessage message)
        {
            // Mevcut session’daki mesaj listesi
            var messages = HttpContext.Session.GetObject<List<ChatMessage>>("ChatMessages") ?? new List<ChatMessage>();

            // Yeni mesaj
            message.Timestamp = DateTime.Now;
            messages.Add(message);

            HttpContext.Session.SetObject("ChatMessages", messages);

            return Ok();
        }

        // [HttpPost]
        // public async Task<IActionResult> Recc(int user_id, int k = 5)
        // {
        //     ViewBag.ScrollToBottom = true;
        //     ViewBag.UserId = user_id;
        //     ViewBag.K = k;

        //     var client = new HttpClient();

        //     var payload = new
        //     {
        //         user_id = user_id,
        //         k = k
        //     };
        //     var json = JsonSerializer.Serialize(payload);
        //     var content = new StringContent(json, Encoding.UTF8, "application/json");

        //     try
        //     {
        //         var response = await client.PostAsync("http://9.223.178.203:5002/recommend", content);
        //         response.EnsureSuccessStatusCode();

        //         var responseBody = await response.Content.ReadAsStringAsync();

        //         var recommendations = JsonSerializer.Deserialize<List<Recommendation>>(responseBody);
        //         TempData["Recommendations"] = JsonSerializer.Serialize(recommendations);
        //         return RedirectToAction("Index");
        //     }
        //     catch (Exception ex)
        //     {
        //         ViewBag.Recommendations = new List<Recommendation>();
        //         ViewBag.Error = ex.Message;
        //     }

        //     return RedirectToAction("Index");
        // }




        // [HttpPost]
        // public async Task<IActionResult> SendMessage(string userMessage)
        // {
        //     if (string.IsNullOrWhiteSpace(userMessage))
        //     {
        //         ViewBag.Response = "Please send a message!";
        //         return View("Index");
        //     }

        //     try
        //     {
        //         var client = _httpClientFactory.CreateClient();
        //         client.Timeout = TimeSpan.FromSeconds(120); // LLM yanıtları uzun sürebilir

        //         var requestBody = new
        //         {
        //             prompt = userMessage,
        //             model = "phi3:mini"
        //         };

        //         var content = new StringContent(
        //             JsonSerializer.Serialize(requestBody),
        //             Encoding.UTF8,
        //             "application/json"
        //         );

        //         // VM içinde Ollama v1 endpoint
        //         var baseUrl = "http://72.146.232.184:11434";

        //         var response = await client.PostAsync($"{baseUrl}/v1/completions", content);

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             ViewBag.Response = $"LLM hatası: {response.StatusCode}";
        //             return View("Index");
        //         }

        //         var responseString = await response.Content.ReadAsStringAsync();

        //         // JSON'dan choices[0].text alanını alıyoruz
        //         using var jsonDoc = JsonDocument.Parse(responseString);
        //         var choices = jsonDoc.RootElement.GetProperty("choices");
        //         ViewBag.Response = choices[0].GetProperty("text").GetString();
        //     }
        //     catch (Exception ex)
        //     {
        //         ViewBag.Response = $"Connection error: {ex.Message}";
        //     }
        //     ViewBag.userMessage = userMessage;
        //     return View("Index");
        // }
    }
}
