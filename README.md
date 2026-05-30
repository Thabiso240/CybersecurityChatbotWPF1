# 🛡️ Cybersecurity Awareness Bot — Part 2 (WPF GUI)

**Student:** Thabiso Nkosi
**Module:** PROG6221
**Part:** 2 — GUI Interface, Keyword Recognition, Sentiment Detection & Memory

---

## 📋 Overview

Part 2 extends the console chatbot from Part 1 into a full **Windows Presentation Foundation (WPF)** application with a polished dark-themed interface. The chatbot now provides keyword-driven cybersecurity education, varies its responses randomly to keep conversations engaging, remembers user details across the session, and adjusts its tone based on detected emotional context.

---

## ✅ Part 2 Requirements Checklist

| Requirement | Status |
|---|---|
| WPF GUI — all Part 1 features translated | ✅ Complete |
| Voice greeting (WAV playback via AudioService) | ✅ Complete |
| ASCII art logo rendered in GUI header | ✅ Complete |
| Keyword recognition — 9 cybersecurity topics | ✅ Complete |
| Random response selection per topic (4 responses each) | ✅ Complete |
| Conversation flow — "tell me more", "give me another tip" | ✅ Complete |
| Memory & recall — name, favourite topic, topics discussed | ✅ Complete |
| Sentiment detection — worried, frustrated, curious, positive | ✅ Complete |
| Error handling & graceful fallback response | ✅ Complete |
| OOP — classes, methods, properties, delegate | ✅ Complete |
| Generic collections — Dictionary, List, Queue | ✅ Complete |
| Delegate — ResponseSelectedHandler event | ✅ Complete |
| Automatic properties in UserMemory | ✅ Complete |
| GitHub Actions CI workflow | ✅ Complete |
| Minimum 6 GitHub commits | Required |

---

## 🚀 How to Run

### Prerequisites
- **Visual Studio 2022** (Community or higher)
- **.NET 8 SDK** — [Download here](https://dotnet.microsoft.com/download/dotnet/8)
- **Windows 10 or Windows 11** (WPF is Windows-only)

### Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/<your-username>/CybersecurityChatbotWPF.git
   cd CybersecurityChatbotWPF
   ```

2. **Open in Visual Studio:**
   Double-click `CybersecurityChatbotWPF.sln`, or use `File → Open → Project/Solution`.

3. **(Optional) Add the voice greeting:**
   Place your recorded `greeting.wav` file in `CybersecurityChatbotWPF/Resources/`.
   In Visual Studio: right-click the file → Properties → Build Action: **Resource**.

4. **Build and run:**
   Press **F5** or click the green **Start** button. The app compiles and opens.

5. **Command-line alternative:**
   ```bash
   dotnet run --project CybersecurityChatbotWPF/CybersecurityChatbotWPF.csproj
   ```

---

## 🖥️ Application Features

### GUI Design
- **Dark cyber-themed interface** with a deep navy and charcoal palette and cyan accents
- **ASCII art header** ported from Part 1 and rendered in the Consolas monospace font
- **Live status bar** showing the current user name, detected mood, and active topic
- **Animated chat bubbles** — bot messages render character by character; user messages fade in
- **Quick-topic chip buttons** for one-click access to all cybersecurity topics
- **Placeholder text** in the input box for usability guidance

### Keyword Recognition (9 Topics)

| Keyword | Description |
|---|---|
| `password` | Strong password practices and management |
| `phishing` | Email and link-based scam awareness |
| `privacy` | Personal data and VPN guidance |
| `malware` | Malware prevention and antivirus advice |
| `scam` | Common scam patterns in South Africa |
| `browsing` | Safe browsing habits |
| `ransomware` | Ransomware defence and backup strategies |
| `2fa` | Two-factor authentication setup |
| `social engineering` | Manipulation attack awareness |

### Random Responses
Each topic has **four distinct responses** chosen at random on every query, so repeated questions receive different tips.

### Conversation Flow
Phrases like `"tell me more"`, `"give me another tip"`, `"continue"`, and `"what else"` return a new tip on the most recently discussed topic. The user does not need to repeat the topic name.

### Memory and Recall
- Remembers the user's **first name** throughout the session
- Remembers the user's **favourite topic** if they say "I'm interested in [topic]"
- Proactively surfaces the favourite topic in fallback responses
- Tracks all topics discussed during the session
- The `ClearSessionTopics()` method resets topic history while preserving the user's name

### Sentiment Detection

| Sentiment | Trigger words | Response prefix |
|---|---|---|
| Worried | worried, scared, afraid, anxious, overwhelmed | Calming and reassuring |
| Frustrated | frustrated, angry, confused, annoyed, lost | Patient and clarifying |
| Curious | curious, interested, excited, learning | Enthusiastic and encouraging |
| Positive | happy, great, good, thanks, helpful | Affirming and energetic |

---

## 📁 Project Structure

```
CybersecurityChatbotWPF/
├── .github/
│   └── workflows/
│       └── ci.yml                       ← GitHub Actions CI pipeline
├── CybersecurityChatbotWPF/
│   ├── Models/
│   │   ├── ChatMessage.cs               ← Message data model
│   │   └── UserMemory.cs                ← User memory with automatic properties
│   ├── Views/
│   │   ├── MainWindow.xaml              ← WPF UI layout and styles
│   │   └── MainWindow.xaml.cs           ← UI code-behind: bubbles, queue, events
│   ├── Resources/
│   │   ├── greeting.wav                 ← Add your recorded WAV file here
│   │   └── README_AUDIO.txt             ← Audio setup instructions
│   ├── App.xaml                         ← Global styles and colour palette
│   ├── App.xaml.cs                      ← Application entry point
│   ├── AudioService.cs                  ← WAV greeting playback (Part 1 port)
│   ├── ResponseEngine.cs                ← Chatbot logic, delegate, keyword engine
│   └── CybersecurityChatbotWPF.csproj
├── CybersecurityChatbotWPF.sln
├── .gitignore
└── README.md
```

---

## 🏗️ Architecture and OOP Design

| Class | Responsibility |
|---|---|
| `ResponseEngine` | All chatbot logic: keyword matching, sentiment, memory, random selection, delegate event |
| `UserMemory` | User data model using automatic properties; stores name, favourite topic, sentiment, topic history |
| `ChatMessage` | Immutable record of a single conversation turn |
| `AudioService` | Static helper for asynchronous WAV file playback via `System.Media.SoundPlayer` |
| `MainWindow` | WPF code-behind: input handling, bubble creation, animation queue, status bar |

### Delegate Usage
`ResponseSelectedHandler` is a custom delegate declared in `ResponseEngine.cs`. The `OnResponseSelected` event fires each time the engine selects a topic-based response. `MainWindow` subscribes to this event to update the Topic label in the status bar — demonstrating event-driven programming without tight coupling between the UI and the engine.

### Generic Collections Used
- `Dictionary<string, List<string>>` — maps each cybersecurity keyword to its pool of responses
- `Dictionary<string, string>` — maps sentiment trigger words to sentiment categories
- `List<string>` — continuation phrases for follow-up detection
- `Queue<string>` — bot message animation queue for sequential rendering

---

## 🔄 GitHub Actions CI

The CI pipeline runs automatically on every push to `main` or `master`.

Steps:
1. Checkout repository
2. Set up .NET 8 SDK
3. Restore NuGet packages
4. Build in Release configuration
5. Verify the `.exe` output exists
6. Upload the build artifact


```

---
<img width="1366" height="768" alt="Screenshot (12)" src="https://github.com/user-attachments/assets/6e7c2374-4433-46d0-8d45-1f0052756c5a" />
<img width="1366" height="768" alt="Screenshot (13)" src="https://github.com/user-attachments/assets/73ce764d-c7fa-4c0e-bdc3-b0403adfb27d" />

## 🎥 Video Presentation

> **YouTube (Unlisted):** [Add your link here after recording]

The video covers:
- Full live demonstration of the running application
- GUI design walkthrough (XAML layout, colour palette, bubble styles)
- Code explanation: ResponseEngine logic, delegate pattern, UserMemory automatic properties
- Demonstration of all Part 2 features: sentiment, memory, keyword recognition, random responses, conversation flow

---


