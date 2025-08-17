
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RagBasedChatbot.Data;
using RagBasedChatbot.Helpers;
using RagBasedChatbot.Models;

namespace RagBasedChatbot.Controllers
{
    public class ChatController : Controller
    {

        private readonly AppDbContext _context;

        public static double CosineSimilarity(List<float> vectorA, List<float> vectorB)
        {
            if (vectorA == null) throw new ArgumentNullException(nameof(vectorA));
            if (vectorB == null) throw new ArgumentNullException(nameof(vectorB));
            if (vectorA.Count != vectorB.Count) throw new ArgumentException("Vectors must be the same length");

            double dot = 0.0;
            double magA = 0.0;
            double magB = 0.0;

            for (int i = 0; i < vectorA.Count; i++)
            {
                dot += vectorA[i] * vectorB[i];
                magA += vectorA[i] * vectorA[i];
                magB += vectorB[i] * vectorB[i];
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

        public ChatController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            var chatHistory = HttpContext.Session.GetObject<List<ChatMessage>>("ChatHistory")
                              ?? new List<ChatMessage>();

            var timeHistory = HttpContext.Session.GetObject<List<MessageTime>>("TimeHistory")
                    ?? new List<MessageTime>();
            ViewBag.ScrollToBottom = true;


            ViewBag.UploadedFileName = HttpContext.Session.GetString("UploadedFileName") ?? "";

            return View((chatHistory, "_", timeHistory));
        }

        [HttpPost]
        public IActionResult Index(string message)
        {
            var chatHistory = HttpContext.Session.GetObject<List<ChatMessage>>("ChatHistory")
                              ?? new List<ChatMessage>();
            var timeHistory = HttpContext.Session.GetObject<List<MessageTime>>("TimeHistory")
                              ?? new List<MessageTime>();

            MessageTime mt = new MessageTime();

            if (!string.IsNullOrEmpty(message))
            {

                var cm = _context.ChatMessages
                    .FirstOrDefault(x => x.QuestionMessage == message);

                if (cm != null)
                {
                    var existing = chatHistory.FirstOrDefault(c => c.MessageId == cm.MessageId);
                    if (existing != null)
                    {
                        chatHistory.Remove(existing);
                        var existing_mt = timeHistory.FirstOrDefault(m => m.MessageId == cm.MessageId);
                        if (existing_mt != null)
                        {
                            timeHistory.Remove(existing_mt);
                        }
                    }

                    chatHistory.Add(cm);
                    mt.MessageId = cm.MessageId;
                    mt.MessageTimeNow = DateTime.Now;
                    timeHistory.Add(mt);
                }
                else
                {
                    ChatMessage notFoundCm = new ChatMessage();
                    if (chatHistory.Count == 0)
                    {
                        notFoundCm.MessageId = (short)(_context.ChatMessages.ToList()[^1].MessageId + 1);
                    }
                    else
                    {
                        notFoundCm.MessageId = (short)(chatHistory[^1].MessageId + 1);
                    }
                    
                    notFoundCm.QuestionMessage = message;
                    notFoundCm.AnswerMessage = "I'm sorry, I don't have an answer to this question.";
                    chatHistory.Add(notFoundCm);

                    MessageTime notFoundMt = new MessageTime();
                    notFoundMt.MessageId = notFoundCm.MessageId;
                    notFoundMt.MessageTimeNow = DateTime.Now;
                    timeHistory.Add(notFoundMt);
                }
            }

            HttpContext.Session.SetObject("ChatHistory", chatHistory);
            HttpContext.Session.SetObject("TimeHistory", timeHistory);
            ViewBag.ScrollToBottom = true;
            ViewBag.UploadedFileName = HttpContext.Session.GetString("UploadedFileName");




            return View((chatHistory, message, timeHistory));
        }


        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile formFile)
        {
            try
            {
                if (formFile != null && formFile.Length > 0)
                {
                    var extension = Path.GetExtension(formFile.FileName);

                    if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest("Only PDF files can be uploaded.");
                    }

                    var randomName = $"{Guid.NewGuid()}{extension}";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", randomName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    string pdfText = PdfHelper.ExtractText(path);
                    var chunks = TextHelper.ChunkText(pdfText);

                    var embeddings = EmbeddingHelper.GetEmbeddings(chunks);

                    EmbeddingHelper.SaveEmbeddings(chunks, embeddings, "wwwroot/pdf/embeddings.json");

                    HttpContext.Session.SetString("UploadedFileName", formFile.FileName);

                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing the file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFile()
        {
            HttpContext.Session.SetString("UploadedFileName", "");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Ask(string message)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return RedirectToAction("Index");
                }

                var queryEmbeddings = EmbeddingHelper.GetEmbeddings(new List<string> { message });

                if (queryEmbeddings == null || queryEmbeddings.Count == 0)
                {
                    return RedirectToAction("Index");
                }

                var queryEmbedding = queryEmbeddings[0];

                var embeddingsPath = "wwwroot\\pdf\\embeddings.json";
                if (!System.IO.File.Exists(embeddingsPath))
                {
                    return RedirectToAction("Index");
                }

                var dbJson = System.IO.File.ReadAllText(embeddingsPath);
                var db = JsonSerializer.Deserialize<List<ChunkEmbedding>>(dbJson) ?? new List<ChunkEmbedding>();

                if (db.Count == 0)
                {
                    return RedirectToAction("Index");
                }

                var topChunks = db
                    .Where(c => c.Embedding != null && c.Text != null)
                    .OrderByDescending(c => CosineSimilarity(queryEmbedding!, c.Embedding!))
                    .Take(5)
                    .Select(c => c.Text);

                if (!topChunks.Any())
                {
                    return RedirectToAction("Index");
                }

                string prompt = $"Based on the following context, provide a clear and direct answer:\n\nContext:\n{string.Join("\n", topChunks)}\n\nQuestion: {message}\nAnswer:";
                string answer = LlmHelper.GetAnswer(prompt);

                var chatHistory = HttpContext.Session.GetObject<List<ChatMessage>>("ChatHistory")
                    ?? new List<ChatMessage>();

                ChatMessage cm = new ChatMessage();
                cm.MessageId = (short)(_context.ChatMessages.ToList()[^1].MessageId + 1);
                cm.AnswerMessage = answer;
                cm.QuestionMessage = message;
                chatHistory.Add(cm);
                HttpContext.Session.SetObject("ChatHistory", chatHistory);

                var timeHistory = HttpContext.Session.GetObject<List<MessageTime>>("TimeHistory")
                    ?? new List<MessageTime>();

                MessageTime mt = new MessageTime();
                mt.MessageId = cm.MessageId;
                mt.MessageTimeNow = DateTime.Now;
                timeHistory.Add(mt);
                HttpContext.Session.SetObject("TimeHistory", timeHistory);

                TempData["Answer"] = answer;
                TempData["Question"] = message;


            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing the file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


    }
}
