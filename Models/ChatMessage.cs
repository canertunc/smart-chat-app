namespace RagBasedChatbot.Models
{
    public class ChatMessage
    {
        public string? QuestionMessage { get; set; } = String.Empty;
        public string? AnswerMessage { get; set; } = String.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}