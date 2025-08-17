
namespace RagBasedChatbot.Helpers
{
    public static class TextHelper
    {
        public static List<string> ChunkText(string text, int chunkSize = 500)
        {
            List<string> chunks = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, text.Length - i);
                chunks.Add(text.Substring(i, length));
            }
            return chunks;
        }
    }
}