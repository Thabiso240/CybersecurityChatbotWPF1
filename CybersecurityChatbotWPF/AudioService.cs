using System.IO;
using System.Media;

namespace CybersecurityChatbotWPF
{
    /// <summary>
    /// Handles playback of the WAV voice greeting on application startup.
    /// Mirrors the AudioPlayer class from Part 1, adapted for a WPF context.
    ///
    /// The greeting.wav file should be placed in:
    ///   CybersecurityChatbotWPF/Resources/greeting.wav
    ///
    /// If the file is absent the application continues silently — audio is
    /// a non-critical enhancement, not a hard dependency.
    /// </summary>
    public static class AudioService
    {
        /// <summary>
        /// Attempts to play the greeting WAV file asynchronously on a background thread
        /// so the UI is not blocked while the audio loads.
        /// </summary>
        public static void PlayGreeting()
        {
            Task.Run(() =>
            {
                try
                {
                    // Search relative to the running executable's folder
                    string[] candidates = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "greeting.wav"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav"),
                        @"Resources\greeting.wav"
                    };

                    foreach (string path in candidates)
                    {
                        if (!File.Exists(path))
                            continue;

                        using SoundPlayer player = new(path);
                        player.PlaySync();  // blocks this background thread, not the UI thread
                        return;
                    }
                    // File not found — app continues without audio
                }
                catch (Exception)
                {
                    // Audio is decorative; suppress all errors silently
                }
            });
        }
    }
}
