using CybersecurityChatbotWPF.Models;

namespace CybersecurityChatbotWPF
{
    // ─────────────────────────────────────────────────────────────────────────────
    // DELEGATE  (Part 2 requirement)
    // Fires each time the engine selects a topic-based response.
    // MainWindow subscribes to update the status bar without direct coupling.
    // ─────────────────────────────────────────────────────────────────────────────
    public delegate void ResponseSelectedHandler(string topic, string response);

    /// <summary>
    /// Core chatbot response engine.
    ///
    /// Responsibilities:
    ///   • Keyword recognition — maps user input to cybersecurity topics
    ///   • Random response selection — keeps conversations varied
    ///   • Conversation flow — handles "tell me more" and follow-up phrases
    ///   • Sentiment detection — adjusts tone based on user mood
    ///   • Memory and recall — personalises responses using stored user data
    ///   • Error handling — graceful default for unrecognised input
    ///   • Delegate event — notifies the UI layer of topic selections
    /// </summary>
    public class ResponseEngine
    {
        // ── Delegate event ───────────────────────────────────────────────────────
        public event ResponseSelectedHandler? OnResponseSelected;

        // ── Dependencies ────────────────────────────────────────────────────────
        private readonly Random      _rng    = new();
        private readonly UserMemory  _memory;

        // ────────────────────────────────────────────────────────────────────────
        // KEYWORD → RESPONSE BANK
        // Each topic has four distinct tips so responses stay fresh on every query.
        // Using Dictionary<string, List<string>> satisfies the generic collection
        // requirement from Part 2.
        // ────────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, List<string>> _topicResponses = new()
        {
            ["password"] = new List<string>
            {
                "🔑 A strong password is at least 12 characters long and mixes uppercase letters, " +
                "lowercase letters, numbers, and symbols. Avoid using names, birthdays, or any word " +
                "found in a dictionary.",

                "🔑 Never reuse the same password across different accounts. If one site is breached, " +
                "attackers try those credentials everywhere. A password manager like Bitwarden (free) " +
                "or 1Password handles this for you.",

                "🔑 Passphrases are both strong and memorable. Something like 'Coffee!Runs@Sunrise7' " +
                "is far harder to crack than 'P@ssw0rd', yet easier to type. Aim for four or more " +
                "random words with symbols.",

                "🔑 Activate two-factor authentication (2FA) on every account that supports it. " +
                "Even if your password leaks, an attacker cannot access your account without the " +
                "second verification step."
            },

            ["phishing"] = new List<string>
            {
                "🎣 Phishing emails manufacture urgency: 'Your account will be suspended in 24 hours!' " +
                "This pressure is deliberate. Pause, breathe, and go directly to the official website " +
                "by typing the address yourself rather than clicking any link in the email.",

                "🎣 Inspect the sender's email address carefully. Criminals register lookalike domains " +
                "such as 'support@paypa1.com' (a '1' instead of 'l') or 'admin@nedbank-support.net'. " +
                "The display name can be anything — the actual domain is what matters.",

                "🎣 Hover over any hyperlink before clicking it. The real destination appears in the " +
                "status bar at the bottom of your browser or email client. If it looks different from " +
                "what is shown in the text, do not click.",

                "🎣 No legitimate organisation — bank, government, or employer — will ever ask for your " +
                "password, PIN, or one-time PIN via email. If you receive such a request, report it " +
                "to the organisation using their official contact number."
            },

            ["privacy"] = new List<string>
            {
                "🔒 Review the privacy settings on all your social media accounts at least once every " +
                "six months. Limit who can see your posts, tag you in photos, or view your friend list " +
                "to people you trust in person.",

                "🔒 Use a VPN (Virtual Private Network) whenever you connect to public Wi-Fi, such as " +
                "in a coffee shop, airport, or mall. A VPN encrypts your traffic so others on the same " +
                "network cannot intercept it.",

                "🔒 Your full name, ID number, home address, and phone number are highly sensitive. " +
                "Think carefully before entering them on any website. Check the site's privacy policy " +
                "to understand how your data will be stored and shared.",

                "🔒 Audit the third-party apps connected to your Google, Facebook, or email account. " +
                "Revoke access for anything you no longer use — each connection is a potential " +
                "entry point for attackers."
            },

            ["malware"] = new List<string>
            {
                "🦠 Keep your operating system, browser, and all applications up to date. Most malware " +
                "exploits known vulnerabilities that security patches already fix. Enable automatic " +
                "updates wherever possible.",

                "🦠 Download software only from official sources: the developer's own website, the " +
                "Microsoft Store, or Google Play. Pirated software and keygens almost always contain " +
                "hidden malware bundled with the installer.",

                "🦠 Install a reputable antivirus application and schedule weekly full-system scans. " +
                "Windows Defender (built into Windows 10/11) is effective when kept up to date and " +
                "costs nothing.",

                "🦠 Treat USB drives from unknown sources as untrusted. Plugging in an unfamiliar " +
                "flash drive can silently install malware on your computer within seconds. If you find " +
                "a stray USB drive, hand it to IT or dispose of it safely."
            },

            ["scam"] = new List<string>
            {
                "⚠️ The golden rule: if an offer sounds too good to be true, it is. Lottery wins, " +
                "prize notifications, and 'Nigerian prince' inheritance schemes are engineered to " +
                "exploit greed and hope. Ignore and delete.",

                "⚠️ If someone calls claiming to be from your bank and asks you to verify card " +
                "details, hang up immediately. Call the number printed on the back of your card " +
                "to confirm whether the contact was genuine.",

                "⚠️ Paying with gift cards is a scam indicator — no legitimate business or government " +
                "department accepts Apple, Google Play, or Takealot gift cards as payment for fees, " +
                "fines, or taxes. It is always a scam.",

                "⚠️ Online romance scams are increasing across South Africa. If someone you have only " +
                "met online quickly professes love and then asks for money — for a plane ticket, a " +
                "medical emergency, or a business deal — it is almost certainly fraud."
            },

            ["browsing"] = new List<string>
            {
                "🌐 Before entering any personal information on a website, check that the address bar " +
                "shows 'https://' and a padlock icon. The 's' stands for secure and means the " +
                "connection is encrypted.",

                "🌐 Consider using a privacy-focused browser such as Firefox or Brave, and install " +
                "the uBlock Origin extension to block ads, tracking scripts, and malicious domains " +
                "automatically.",

                "🌐 Clear your browser cookies and cache every few weeks to remove stored tracking " +
                "data. In Chrome, Ctrl+Shift+Del opens the clear browsing data dialog directly.",

                "🌐 Avoid accessing banking, email, or any sensitive account on a public or shared " +
                "computer. If you must, use the browser's private/incognito mode and log out " +
                "completely when finished."
            },

            ["ransomware"] = new List<string>
            {
                "💾 Ransomware encrypts your files and demands payment for the decryption key. " +
                "The only reliable defence is a regular offline backup — an external drive " +
                "disconnected from your computer, or a cloud backup service like Backblaze.",

                "💾 Never open email attachments you were not expecting, especially .zip archives, " +
                ".exe files, or Office documents that ask you to 'Enable Macros'. These are among " +
                "the most common ransomware delivery methods.",

                "💾 If your device becomes infected with ransomware, disconnect it from the internet " +
                "and all network shares immediately. This limits how far the encryption spreads. " +
                "Do not pay the ransom — it funds criminals and does not guarantee recovery."
            },

            ["2fa"] = new List<string>
            {
                "📱 Two-Factor Authentication (2FA) adds a second verification step after your " +
                "password. Even if an attacker steals your credentials, they cannot log in without " +
                "also having physical access to your phone or authenticator app.",

                "📱 Prefer an authenticator app (Google Authenticator, Authy, or Microsoft " +
                "Authenticator) over SMS-based 2FA. SMS messages can be intercepted through " +
                "SIM-swap attacks, which are increasingly common in South Africa.",

                "📱 Enable 2FA on your most critical accounts first: email (which is used to reset " +
                "everything else), online banking, and social media. Most major services support " +
                "it under Settings → Security."
            },

            ["social engineering"] = new List<string>
            {
                "🎭 Social engineering exploits human psychology rather than technical vulnerabilities. " +
                "Attackers build trust or create panic to make you bypass your own judgement. " +
                "Always verify the identity of anyone requesting sensitive information.",

                "🎭 Attackers often impersonate IT helpdesk staff, calling to say your account has " +
                "been compromised and asking for your credentials to 'fix' the problem. Real IT teams " +
                "never ask for your password — change it and report the call.",

                "🎭 Pretexting is when a scammer fabricates a believable scenario — posing as a " +
                "new colleague, a courier, or a government official — to justify their request. " +
                "Confirm identities through official channels before acting on any unusual request."
            }
        };

        // ────────────────────────────────────────────────────────────────────────
        // SENTIMENT MAP
        // Maps individual words/phrases to a sentiment category.
        // ────────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentMap = new()
        {
            ["worried"]       = "worried",
            ["scared"]        = "worried",
            ["afraid"]        = "worried",
            ["nervous"]       = "worried",
            ["anxious"]       = "worried",
            ["overwhelmed"]   = "worried",
            ["frustrated"]    = "frustrated",
            ["angry"]         = "frustrated",
            ["annoyed"]       = "frustrated",
            ["confused"]      = "frustrated",
            ["lost"]          = "frustrated",
            ["curious"]       = "curious",
            ["interested"]    = "curious",
            ["want to know"]  = "curious",
            ["excited"]       = "curious",
            ["learning"]      = "curious",
            ["happy"]         = "positive",
            ["great"]         = "positive",
            ["good"]          = "positive",
            ["thanks"]        = "positive",
            ["helpful"]       = "positive"
        };

        // ────────────────────────────────────────────────────────────────────────
        // SENTIMENT PREFIXES
        // Prepended to topic responses when a sentiment is detected, making the
        // bot feel empathetic rather than mechanical.
        // ────────────────────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentPrefixes = new()
        {
            ["worried"]    = "It's completely understandable to feel that way — " +
                             "cybersecurity threats are real and can be unsettling. " +
                             "Let me help put your mind at ease.\n\n",

            ["frustrated"] = "I hear you, and I want to help make sense of this. " +
                             "Let's work through it step by step.\n\n",

            ["curious"]    = "Great mindset! Curiosity is one of the strongest defences " +
                             "against cyber threats. Here is what you should know:\n\n",

            ["positive"]   = "Love the energy! Staying informed is the first step to staying " +
                             "protected. Here is a tip for you:\n\n",

            ["neutral"]    = ""
        };

        // ────────────────────────────────────────────────────────────────────────
        // FOLLOW-UP PHRASES
        // When any of these appear and no specific keyword is present, the bot
        // continues on the last topic without the user needing to repeat it.
        // ────────────────────────────────────────────────────────────────────────
        private readonly List<string> _continuationPhrases = new()
        {
            "tell me more", "more", "explain more", "give me another", "another tip",
            "more tips", "go on", "continue", "keep going", "what else", "and then",
            "elabor", "expand"
        };

        // ────────────────────────────────────────────────────────────────────────
        // CONSTRUCTOR
        // ────────────────────────────────────────────────────────────────────────
        public ResponseEngine(UserMemory memory)
        {
            _memory = memory;
        }

        // ────────────────────────────────────────────────────────────────────────
        // PUBLIC: GetResponse — main entry point for the conversation
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates an appropriate response for the given user input.
        /// Processing order:
        ///   1. Input validation
        ///   2. Sentiment detection
        ///   3. Follow-up / continuation check
        ///   4. Interest/favourite topic declaration
        ///   5. Name introduction handling
        ///   6. General conversational responses
        ///   7. Cybersecurity keyword matching (main logic)
        ///   8. Proactive memory recall
        ///   9. Default fallback
        /// </summary>
        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "Please type a message and I will do my best to help. 😊";

            string input = userInput.ToLower().Trim();

            // ── Step 1: Detect and store sentiment ──────────────────────────────
            DetectSentiment(input);

            // ── Step 2: Follow-up / continuation ────────────────────────────────
            // Only activates when no specific topic keyword is present in the input.
            bool hasKeyword = _topicResponses.Keys.Any(k => input.Contains(k));

            if (!hasKeyword && _continuationPhrases.Any(p => input.Contains(p)))
            {
                if (!string.IsNullOrWhiteSpace(_memory.LastTopic) &&
                    _topicResponses.TryGetValue(_memory.LastTopic, out var tipList))
                {
                    string tip      = tipList[_rng.Next(tipList.Count)];
                    string response = SentimentPrefix() + tip;
                    OnResponseSelected?.Invoke(_memory.LastTopic, response);
                    return response;
                }
                return "Sure! Which topic would you like to explore further?\n\n" +
                       "Try: passwords, phishing, privacy, malware, scams, browsing, " +
                       "ransomware, 2FA, or social engineering.";
            }

            // ── Step 3: User declares a favourite topic ──────────────────────────
            if (input.Contains("i'm interested in")  ||
                input.Contains("i am interested in") ||
                input.Contains("my favourite topic") ||
                input.Contains("i like learning about"))
            {
                foreach (string keyword in _topicResponses.Keys)
                {
                    if (!input.Contains(keyword)) continue;

                    _memory.FavouriteTopic = keyword;
                    _memory.RecordTopic(keyword);

                    string tip = GetRandomTip(keyword);
                    string favResponse =
                        $"Noted! I will remember that you have a particular interest in {keyword}. " +
                        "It is one of the most important areas of cybersecurity. " +
                        "Here is a tip to get you started:\n\n" + tip;

                    OnResponseSelected?.Invoke(keyword, favResponse);
                    return favResponse;
                }
            }

            // ── Step 4: Name introduction ────────────────────────────────────────
            if (input.Contains("my name is") ||
                (input.StartsWith("i am ")   && input.Length > 5) ||
                (input.StartsWith("i'm ")    && input.Length > 4))
            {
                string[] words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    string name = CapFirst(words[^1]);
                    if (!name.Equals(_memory.UserName, StringComparison.OrdinalIgnoreCase))
                    {
                        _memory.UserName = name;
                        return $"Nice to meet you, {_memory.UserName}! 👋 " +
                               "I am your Cybersecurity Awareness Assistant. " +
                               "Ask me about passwords, phishing, privacy, malware, scams, " +
                               "2FA, or social engineering whenever you are ready.";
                    }
                }
            }

            // ── Step 5: General conversational responses ─────────────────────────
            if (input.Contains("how are you") || input.Contains("how r u"))
                return "Fully operational and here to keep you safe! 🤖 What would you like to explore today?";

            if (input.Contains("what can you do") ||
                input.Contains("what can i ask")  ||
                input.Contains("your purpose")    ||
                input.Contains("help me"))
                return BuildHelpResponse();

            if (input == "hi"            ||
                input == "hello"         ||
                input.StartsWith("hi ")  ||
                input.Contains("hello ") ||
                input.Contains("hey"))
                return $"Hello, {_memory.UserName}! 👋 What cybersecurity topic can I help you with today?";

            if (input.Contains("thank"))
            {
                string closing = _memory.HasFavouriteTopic
                    ? $" Since you have an interest in {_memory.FavouriteTopic}, " +
                      "it is worth revisiting your security settings on that topic regularly."
                    : " Feel free to ask anything else — staying informed is the best defence.";
                return $"You are very welcome, {_memory.UserName}! 😊" + closing;
            }

            if (input.Contains("bye") || input.Contains("goodbye") || input.Contains("exit"))
                return $"Take care, {_memory.UserName}! 👋 Remember: think before you click, " +
                       "and stay vigilant out there.";

            // ── Step 6: Cybersecurity keyword matching ───────────────────────────
            foreach (string keyword in _topicResponses.Keys)
            {
                if (!input.Contains(keyword)) continue;

                _memory.RecordTopic(keyword);
                string tip    = GetRandomTip(keyword);
                string prefix = SentimentPrefix();

                // Memory personalisation: if this is the user's favourite topic, acknowledge it
                string memNote = (_memory.HasFavouriteTopic &&
                                  _memory.FavouriteTopic == keyword)
                    ? $"As someone who is particularly interested in {keyword}, " +
                      "this is especially relevant for you.\n\n"
                    : string.Empty;

                string full = prefix + memNote + tip;
                OnResponseSelected?.Invoke(keyword, full);
                return full;
            }

            // ── Step 7: Proactive memory recall for unknown input ────────────────
            if (_memory.HasFavouriteTopic && _rng.Next(3) == 0)
            {
                string proactiveTip = GetRandomTip(_memory.FavouriteTopic);
                return $"I am not quite sure what you mean, {_memory.UserName}. " +
                       $"Could you rephrase that? 🤔\n\n" +
                       $"In the meantime, here is a tip on {_memory.FavouriteTopic} " +
                       $"since I know that interests you:\n\n{proactiveTip}";
            }

            // ── Step 8: Default fallback ─────────────────────────────────────────
            return $"I did not quite follow that, {_memory.UserName}. Could you rephrase? 🤔\n\n" +
                   "You can ask about: passwords, phishing, privacy, malware, scams, " +
                   "safe browsing, ransomware, 2FA, or social engineering.";
        }

        // ────────────────────────────────────────────────────────────────────────
        // PUBLIC UTILITIES
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resets the detected sentiment to neutral after the UI has read it.
        /// Called by MainWindow immediately after updating the status bar.
        /// </summary>
        public void ResetSentiment() => _memory.DetectedSentiment = "neutral";

        /// <summary>
        /// Exposes the list of known topics for use by the quick-topic button builder.
        /// </summary>
        public IEnumerable<string> GetTopics() => _topicResponses.Keys;

        // ────────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ────────────────────────────────────────────────────────────────────────

        private string GetRandomTip(string keyword)
        {
            if (_topicResponses.TryGetValue(keyword, out List<string>? tips))
                return tips[_rng.Next(tips.Count)];
            return string.Empty;
        }

        /// <summary>
        /// Scans the input for sentiment keywords and stores the result in memory.
        /// The first match wins; neutral is only set if nothing is found.
        /// </summary>
        private void DetectSentiment(string input)
        {
            foreach (KeyValuePair<string, string> entry in _sentimentMap)
            {
                if (input.Contains(entry.Key))
                {
                    _memory.DetectedSentiment = entry.Value;
                    return;
                }
            }
            _memory.DetectedSentiment = "neutral";
        }

        /// <summary>
        /// Returns the sentiment-aware prefix for the current detected sentiment,
        /// or an empty string if sentiment is neutral.
        /// </summary>
        private string SentimentPrefix()
        {
            if (_sentimentPrefixes.TryGetValue(_memory.DetectedSentiment, out string? prefix))
                return prefix;
            return string.Empty;
        }

        private string BuildHelpResponse()
        {
            return "I am your Cybersecurity Awareness Assistant 🛡️\n\n" +
                   "I can educate you on the following topics:\n" +
                   "  • Password safety and management\n" +
                   "  • Phishing and email scams\n" +
                   "  • Privacy and personal data protection\n" +
                   "  • Malware and antivirus practices\n" +
                   "  • Online scams common in South Africa\n" +
                   "  • Safe browsing habits\n" +
                   "  • Ransomware defence\n" +
                   "  • Two-factor authentication (2FA)\n" +
                   "  • Social engineering attacks\n\n" +
                   "I also remember what you tell me and adjust my tone based on how you are feeling. " +
                   "Just ask — there are no silly questions when it comes to staying safe online!";
        }

        private static string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s[1..].ToLower();
        }
    }
}
