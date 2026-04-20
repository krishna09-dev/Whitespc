# 📝 Whitespc — Offline Journal App

**Whitespc** is a **.NET MAUI Blazor Hybrid desktop application** built for **private, offline personal journaling**.  
All data stays **on your device** — no cloud, no accounts, no internet required.
 
---

## ✨ Features
- 📅 One journal entry per day
- 😊 Mood tracking (primary + secondary)
- 🏷 Tags, favorites, pin & archive
- 🔐 Optional PIN protection with auto-lock
- 🌗 Light / Dark mode
- 📄 Export journal entries as PDF
- 🧪 Demo data generator
- 🗑 Full database wipe (Danger Zone)

---

## 🛠 Tech Stack
- .NET 9
- .NET MAUI Blazor Hybrid
- C#
- SQLite + Entity Framework Core
- Mac Catalyst (macOS)

---

## 📁 Project Structure
```
Components/    Blazor UI (Pages, Layouts, Shared)
Models/        Data models
Services/      Business logic & security
Data/          SQLite DbContext
Platforms/     macOS / Mac Catalyst services
```

---

## 🚀 Build (macOS)
```bash
dotnet restore
dotnet publish -c Release -f net9.0-maccatalyst18.0
```

Output:
```
bin/Release/net9.0-maccatalyst18.0/whitespc.app
```

---

## 🔒 Privacy
- 100% offline
- No tracking, no analytics
- Data stored locally using SQLite

---

## 🎓 Purpose
Built as a **university coursework / portfolio project**, focusing on:
- Offline-first design
- Secure local data
- Clean architecture

---

❤️ Built for mindful journaling.
