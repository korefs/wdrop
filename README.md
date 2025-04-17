# 📦 wdrop

A lightweight and fast terminal to **share files on the local network** with just one command.

---

## How to use

1. Compile the project:
* MacOS
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./out
```
* Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./out
```

2. Run
```bash
wdrop file.zip
```

## Requirements
.NET 8 SDK

## ✅ Current features

Share any file over local HTTP

Interactive file selection in the terminal

Zero external dependencies to download

Standalone binary support (PublishSingleFile)

## 🛠️ In development / Future features

📂 Folder support (with automatic zip before serving)

📄 Multiple file support

🧾 HTML listing of available files (server mode)

🔐 Security token or automatic expiration

📱 QR Code generation with the link to facilitate downloading on mobile

🌍 Create a client to support P2P connection

🌍 External network support via tunneling (e.g. ngrok, Cloudflare Tunnel)

🎨 More stylish TUI interface ("server" mode with live control)

🛑 Self-destruct mode after download