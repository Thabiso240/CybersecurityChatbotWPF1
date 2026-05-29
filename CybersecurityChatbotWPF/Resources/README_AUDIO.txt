## Voice Greeting — Audio Setup

Place your recorded WAV file in this folder with the filename:

    greeting.wav

### Requirements
- Format : WAV (PCM, 16-bit, 44.1 kHz recommended)
- Duration: 5–10 seconds
- Content : e.g. "Hello! Welcome to the Cybersecurity Awareness Bot.
             I'm here to help you stay safe online."

### Recording (Windows)
1. Open the Voice Recorder app (search for it in the Start menu).
2. Record your greeting and save.
3. Use Audacity (free) to export it as WAV if the default format differs.

### How it is used
AudioService.PlayGreeting() searches for this file relative to the
executable on startup and plays it asynchronously using System.Media.SoundPlayer.
If the file is absent the application starts normally without audio.

### Visual Studio — mark as Resource
1. Right-click greeting.wav in Solution Explorer.
2. Properties → Build Action → Resource.
3. This embeds the file in the output directory automatically.
