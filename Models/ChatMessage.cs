namespace RagBasedChatbot.Models
{
    public class ChatMessage
    {
        
        public int MessageId { get; set; }
        public string? QuestionMessage { get; set; } = String.Empty;
        public string? AnswerMessage { get; set; } = String.Empty;

    }

}