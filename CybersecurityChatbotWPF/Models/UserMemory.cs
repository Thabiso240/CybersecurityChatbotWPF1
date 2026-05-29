namespace CybersecurityChatbotWPF.Models
{
    /// <summary>
    /// Stores remembered details about the user across the conversation session.
    /// Uses automatic properties (Part 1 requirement) to expose user-specific data
    /// and drives personalised responses throughout the chat.
    /// </summary>
    public class UserMemory
    {
        // ── Automatic Properties ─────────────────────────────────────────────────

        /// <summary>The user's first name, collected at the start of the session.</summary>
        public string UserName { get; set; } = "User";

        /// <summary>
        /// The cybersecurity topic the user expressed the most interest in.
        /// Set when the user says "I'm interested in [topic]".
        /// </summary>
        public string FavouriteTopic { get; set; } = string.Empty;

        /// <summary>The last cybersecurity topic that was discussed.</summary>
        public string LastTopic { get; set; } = string.Empty;

        /// <summary>
        /// The sentiment detected in the user's most recent message.
        /// Possible values: "worried", "frustrated", "curious", "positive", "neutral".
        /// </summary>
        public string DetectedSentiment { get; set; } = "neutral";

        /// <summary>All topics that have been raised at least once in the session.</summary>
        public List<string> TopicsDiscussed { get; set; } = new();

        // ── Computed Properties ──────────────────────────────────────────────────

        /// <summary>Returns true if the user has declared a favourite topic.</summary>
        public bool HasFavouriteTopic => !string.IsNullOrWhiteSpace(FavouriteTopic);

        /// <summary>Returns true if any topics have been discussed this session.</summary>
        public bool HasDiscussedTopics => TopicsDiscussed.Count > 0;

        // ── Methods ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Marks a topic as discussed and updates the last-active topic.
        /// Avoids duplicate entries in the list.
        /// </summary>
        public void RecordTopic(string topic)
        {
            LastTopic = topic;
            if (!TopicsDiscussed.Contains(topic))
                TopicsDiscussed.Add(topic);
        }

        /// <summary>
        /// Resets the session to defaults while preserving the user's name.
        /// Used when the chat is cleared so the bot still recognises the user.
        /// </summary>
        public void ClearSessionTopics()
        {
            LastTopic      = string.Empty;
            FavouriteTopic = string.Empty;
            TopicsDiscussed.Clear();
            DetectedSentiment = "neutral";
        }
    }
}
