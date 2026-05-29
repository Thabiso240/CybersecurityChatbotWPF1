namespace CybersecurityChatbotWPF.Models
{
    /// <summary>
    /// Represents a single message in the conversation.
    /// Holds the sender identity, message text, and the time it was sent.
    /// </summary>
    public class ChatMessage
    {
        public string Sender    { get; set; }
        public string Message   { get; set; }
        public string Timestamp { get; set; }

        public bool IsBot => Sender == "Bot";

        public ChatMessage(string sender, string message)
        {
            Sender    = sender;
            Message   = message;
            Timestamp = DateTime.Now.ToString("HH:mm");
        }
    }
}
