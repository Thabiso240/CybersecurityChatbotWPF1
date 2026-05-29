using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CybersecurityChatbotWPF.Models;

namespace CybersecurityChatbotWPF.Views
{
    /// <summary>
    /// Code-behind for the main application window.
    ///
    /// Responsibilities:
    ///   • Wires the UI controls to the ResponseEngine
    ///   • Manages the chat message queue and animated typing effect
    ///   • Builds chat bubbles dynamically in code (no data-binding required)
    ///   • Handles the name-collection startup flow
    ///   • Updates the status bar via the delegate event from ResponseEngine
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Core engine and memory ────────────────────────────────────────────────
        private readonly UserMemory     _memory;
        private readonly ResponseEngine _engine;

        // ── Typing-animation queue ────────────────────────────────────────────────
        // Stores pending messages so they appear sequentially rather than all at once.
        private readonly Queue<string> _botQueue = new();
        private bool _isAnimating = false;

        // ── Startup state ─────────────────────────────────────────────────────────
        private bool _awaitingName = true;

        // ─────────────────────────────────────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────────────────────────────────────

        public MainWindow()
        {
            InitializeComponent();

            _memory = new UserMemory();
            _engine = new ResponseEngine(_memory);

            // Subscribe to the delegate event for status bar updates (Part 2 delegate requirement)
            _engine.OnResponseSelected += HandleResponseSelected;

            // Play the WAV greeting from Part 1 on startup
            AudioService.PlayGreeting();

            // Defer UI initialisation until the window is fully rendered
            Loaded += OnWindowLoaded;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STARTUP
        // ─────────────────────────────────────────────────────────────────────────

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            BuildTopicButtons();
            ShowWelcomeMessages();
        }

        private void ShowWelcomeMessages()
        {
            QueueBotMessage(
                "╔══════════════════════════════════════════════════╗\n" +
                "║   Welcome to the Cybersecurity Awareness Bot!   ║\n" +
                "║   Your trusted guide to staying safe online.    ║\n" +
                "╚══════════════════════════════════════════════════╝");

            QueueBotMessage(
                "I am here to educate you about cybersecurity topics relevant to South Africa — " +
                "including passwords, phishing, privacy, malware, scams, safe browsing, " +
                "ransomware, two-factor authentication, and social engineering. 🛡️");

            QueueBotMessage("Before we begin, could you please tell me your name?");

            FlushQueue();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // MESSAGE QUEUE — ensures bot messages appear in sequence
        // ─────────────────────────────────────────────────────────────────────────

        private void QueueBotMessage(string text) => _botQueue.Enqueue(text);

        /// <summary>
        /// Starts draining the message queue. Each call to this method is safe to
        /// make even if an animation is already in progress — the timer callback
        /// calls FlushQueue() when it finishes, creating the sequential chain.
        /// </summary>
        private void FlushQueue()
        {
            if (_isAnimating || _botQueue.Count == 0)
                return;

            _isAnimating = true;
            string next  = _botQueue.Dequeue();
            AnimateBotMessage(next);
        }

        /// <summary>
        /// Renders a bot message with a character-by-character typing effect.
        /// Uses DispatcherTimer so all UI operations remain on the UI thread.
        /// </summary>
        private void AnimateBotMessage(string fullText)
        {
            Border  bubble       = BuildBotBubble(out TextBlock contentBlock);
            ChatPanel.Children.Add(bubble);
            ScrollToBottom();

            int index = 0;
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(10) };

            timer.Tick += (_, _) =>
            {
                if (index < fullText.Length)
                {
                    contentBlock.Text += fullText[index];
                    index++;
                    if (index % 12 == 0) ScrollToBottom();
                }
                else
                {
                    timer.Stop();
                    _isAnimating = false;
                    ScrollToBottom();
                    FlushQueue();  // advance to the next queued message
                }
            };

            timer.Start();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // SEND / INPUT HANDLING
        // ─────────────────────────────────────────────────────────────────────────

        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            InputBox.Clear();
            PlaceholderText.Visibility = Visibility.Visible;

            AppendUserBubble(text);

            // ── Name-collection flow ───────────────────────────────────────────
            if (_awaitingName)
            {
                string name = ExtractFirstWord(text);
                _memory.UserName   = name;
                _awaitingName      = false;
                UserNameLabel.Text = name;

                QueueBotMessage(
                    $"Great to meet you, {name}! 👋 I am your Cybersecurity Awareness Assistant.\n\n" +
                    "You can ask me about passwords, phishing, privacy, malware, scams, safe browsing, " +
                    "ransomware, 2FA, or social engineering. Use the quick-topic buttons below " +
                    "or just type your question.");

                QueueBotMessage("What would you like to learn about first?");
                FlushQueue();
                return;
            }

            // ── Normal conversation ────────────────────────────────────────────
            string response = _engine.GetResponse(text);
            RefreshStatusBar();
            QueueBotMessage(response);
            FlushQueue();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(InputBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _memory.ClearSessionTopics();
            TopicLabel.Text    = "None yet";
            MoodLabel.Text     = "😐 Neutral";

            QueueBotMessage(
                $"Chat cleared! I still remember who you are, {_memory.UserName}. 😊 " +
                "What topic would you like to cover next?");
            FlushQueue();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // QUICK TOPIC BUTTONS
        // ─────────────────────────────────────────────────────────────────────────

        private void BuildTopicButtons()
        {
            var topics = new List<(string Label, string Query)>
            {
                ("🔑 Passwords",          "Tell me about password safety"),
                ("🎣 Phishing",           "Tell me about phishing"),
                ("🔒 Privacy",            "Tell me about privacy"),
                ("🦠 Malware",            "Tell me about malware"),
                ("⚠️ Scams",              "Tell me about scams"),
                ("🌐 Safe Browsing",      "Tell me about safe browsing"),
                ("💾 Ransomware",         "Tell me about ransomware"),
                ("📱 2FA",                "Tell me about 2FA"),
                ("🎭 Social Engineering", "Tell me about social engineering"),
                ("💡 More Tips",          "Give me another tip"),
            };

            foreach ((string label, string query) in topics)
            {
                Button btn = new()
                {
                    Content = label,
                    Style   = (Style)FindResource("TopicButton"),
                    Tag     = query
                };
                btn.Click += TopicButton_Click;
                TopicButtonsPanel.Children.Add(btn);
            }
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string query })
            {
                InputBox.Text = query;
                SendMessage();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // DELEGATE CALLBACK
        // Fires on the background thread when ResponseEngine picks a topic response.
        // Dispatches to UI thread to update the Topic label in the status bar.
        // ─────────────────────────────────────────────────────────────────────────

        private void HandleResponseSelected(string topic, string response)
        {
            Dispatcher.InvokeAsync(() =>
            {
                TopicLabel.Text = CapFirst(topic);
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STATUS BAR UPDATE
        // ─────────────────────────────────────────────────────────────────────────

        private void RefreshStatusBar()
        {
            UserNameLabel.Text = _memory.UserName;

            // Map sentiment to a display label and colour
            (string moodText, Color moodColour) = _memory.DetectedSentiment switch
            {
                "worried"    => ("😟 Worried",    Color.FromRgb(0xFF, 0x44, 0x44)),
                "frustrated" => ("😤 Frustrated", Color.FromRgb(0xFF, 0xA5, 0x00)),
                "curious"    => ("🤔 Curious",    Color.FromRgb(0x00, 0xD4, 0xFF)),
                "positive"   => ("😊 Positive",   Color.FromRgb(0x00, 0xFF, 0x88)),
                _            => ("😐 Neutral",    Color.FromRgb(0x8B, 0x94, 0x9E))
            };

            MoodLabel.Text       = moodText;
            MoodLabel.Foreground = new SolidColorBrush(moodColour);

            if (!string.IsNullOrWhiteSpace(_memory.LastTopic))
                TopicLabel.Text = CapFirst(_memory.LastTopic);

            // Sentiment has been consumed — reset to neutral so it does not persist
            _engine.ResetSentiment();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // BUBBLE BUILDERS — construct WPF UI elements for chat messages
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a bot message bubble and exposes the inner TextBlock for animation.
        /// The bubble has a left-sided cyan border and fades in on appearance.
        /// </summary>
        private Border BuildBotBubble(out TextBlock contentBlock)
        {
            contentBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Foreground   = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
                FontFamily   = new FontFamily("Consolas"),
                FontSize     = 13,
                LineHeight   = 21
            };

            TextBlock label = new()
            {
                Text       = "🤖 CyberBot",
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                FontSize   = 10,
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 5)
            };

            TextBlock timestamp = new()
            {
                Text       = DateTime.Now.ToString("HH:mm"),
                Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x94, 0x9E)),
                FontSize   = 10,
                Margin     = new Thickness(0, 5, 0, 0)
            };

            StackPanel inner = new();
            inner.Children.Add(label);
            inner.Children.Add(contentBlock);
            inner.Children.Add(timestamp);

            Border bubble = new()
            {
                Background      = new SolidColorBrush(Color.FromRgb(0x1C, 0x21, 0x28)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF)),
                BorderThickness = new Thickness(2, 0, 0, 0),
                CornerRadius    = new CornerRadius(0, 8, 8, 8),
                Padding         = new Thickness(14, 10, 14, 10),
                Margin          = new Thickness(0, 6, 90, 6),
                MaxWidth        = 680,
                Child           = inner,
                Opacity         = 0
            };

            DoubleAnimation fadeIn = new(0, 1, TimeSpan.FromMilliseconds(220));
            bubble.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            return bubble;
        }

        /// <summary>
        /// Builds and immediately appends a user message bubble (right-aligned, blue).
        /// User messages do not animate — they appear instantly.
        /// </summary>
        private void AppendUserBubble(string text)
        {
            TextBlock contentBlock = new()
            {
                Text         = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground   = new SolidColorBrush(Colors.White),
                FontFamily   = new FontFamily("Consolas"),
                FontSize     = 13,
                LineHeight   = 21
            };

            TextBlock label = new()
            {
                Text                = $"👤 {_memory.UserName}",
                Foreground          = new SolidColorBrush(Color.FromRgb(0xB0, 0xC8, 0xFF)),
                FontSize            = 10,
                FontWeight          = FontWeights.Bold,
                Margin              = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            TextBlock timestamp = new()
            {
                Text                = DateTime.Now.ToString("HH:mm"),
                Foreground          = new SolidColorBrush(Color.FromRgb(0xB0, 0xC8, 0xFF)),
                FontSize            = 10,
                Margin              = new Thickness(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            StackPanel inner = new();
            inner.Children.Add(label);
            inner.Children.Add(contentBlock);
            inner.Children.Add(timestamp);

            Border bubble = new()
            {
                Background          = new SolidColorBrush(Color.FromRgb(0x1F, 0x6F, 0xEB)),
                CornerRadius        = new CornerRadius(8, 8, 0, 8),
                Padding             = new Thickness(14, 10, 14, 10),
                Margin              = new Thickness(90, 6, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth            = 680,
                Child               = inner,
                Opacity             = 0
            };

            DoubleAnimation fadeIn = new(0, 1, TimeSpan.FromMilliseconds(150));
            bubble.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────────

        private void ScrollToBottom()
        {
            Dispatcher.InvokeAsync(
                () => ChatScrollViewer.ScrollToEnd(),
                DispatcherPriority.Background);
        }

        private static string ExtractFirstWord(string input)
        {
            string[] words = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "User";
            string name = CapFirst(words[0]);
            // Strip punctuation that should not be part of a name
            return new string(name.Where(c => char.IsLetter(c)).ToArray()) is { Length: > 0 } clean
                ? clean
                : "User";
        }

        private static string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s[1..].ToLower();
        }
    }
}
