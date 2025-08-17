using System.Diagnostics;
using System.Text.Json;

namespace RagBasedChatbot.Helpers
{
    public static class EmbeddingHelper
    {
        public static List<List<float>> GetEmbeddings(List<string> chunks)
        {
            string chunksJson = JsonSerializer.Serialize(chunks);

            var psi = new ProcessStartInfo
            {
                FileName = @"C:\Users\caner\Desktop\rag-based-chatbot-with-psql\RagBasedChatbot\Helpers\venv\Scripts\python.exe",
                Arguments = @"Embeeding.py",
                WorkingDirectory = @"C:\Users\caner\Desktop\rag-based-chatbot-with-psql\RagBasedChatbot\Helpers",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardInputEncoding = new System.Text.UTF8Encoding(false), // BOM olmadan UTF8
                StandardOutputEncoding = new System.Text.UTF8Encoding(false)
            };

            var process = Process.Start(psi) ?? throw new InvalidOperationException("Process couldn't start.");
            
            // stdin üzerinden JSON gönder
            process.StandardInput.Write(chunksJson);
            process.StandardInput.Flush();
            process.StandardInput.Close();

            string output = "";
            string error = "";
            
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            output = outputTask.Result;
            error = errorTask.Result;

            try
            {
                var embeddings = JsonSerializer.Deserialize<List<List<float>>>(output.Trim())
                               ?? new List<List<float>>();
                return embeddings;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON returned from Python script. Output: '{output}'. Error: {ex.Message}");
            }
        }

        public static void SaveEmbeddings(List<string> chunks, List<List<float>> embeddings, string path)
        {

            var db = new List<ChunkEmbedding>();
            
            for (int i = 0; i < chunks.Count; i++)
            {
                db.Add(new ChunkEmbedding
                {
                    Text = chunks[i],
                    Embedding = embeddings[i]
                });
            }

            File.WriteAllText(path, JsonSerializer.Serialize(db));
        }
    }

    public class ChunkEmbedding
    {
        public string? Text { get; set; }
        public List<float>? Embedding { get; set; }
    }
}