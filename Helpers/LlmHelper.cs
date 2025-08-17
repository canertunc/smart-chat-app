using System.Diagnostics;
using System.Text.Json;

namespace RagBasedChatbot.Helpers
{
    public static class LlmHelper
    {
        public static string GetAnswer(string prompt)
        {
            try
            {
                string escapedPrompt = prompt.Replace("\"", "\\\"");
                
                var psi = new ProcessStartInfo
                {
                    FileName = "C:\\Users\\caner\\Desktop\\rag-based-chatbot-with-psql\\RagBasedChatbot\\Helpers\\venv\\Scripts\\python.exe",
                    Arguments = $"local_llm.py \"{escapedPrompt}\"",
                    WorkingDirectory = "C:\\Users\\caner\\Desktop\\rag-based-chatbot-with-psql\\RagBasedChatbot\\Helpers",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = new System.Text.UTF8Encoding(false),
                    StandardErrorEncoding = new System.Text.UTF8Encoding(false)
                };

                var process = Process.Start(psi) ?? throw new InvalidOperationException("Python process couldn't start.");

                if (!process.WaitForExit(30000000)) 
                {
                    process.Kill();
                    throw new TimeoutException("LLM process timed out after 5 minutes");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();


                return output.Trim();
            }
            catch (Exception)
            {
                return "Sorry, an error occurred while generating the response.";
            }
        }
    }
}
