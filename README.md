# 📦 wdrop

A lightweight and fast terminal to **share files or entire folders** with a single command.

```bash
 _    _     _                 
| |  | |   | |                
| |  | | __| |_ __ ___  _ __  
| |/\| |/ _` | '__/ _ \| '_ \ 
\  /\  / (_| | | | (_) | |_) |
 \/  \/ \__,_|_|  \___/| .__/ 
                       | |    
                       |_|
```

---

## ✅ Features

- ⚡ Simple CLI interface

- 🌐 Automatic detection of your local IP

- 🔗 Instant HTTP link to download the file

- 📂 Interactive file/folder picker if no argument is provided

- 🗂️ Folder support via auto-zipping

- 🧪 Cross-platform (Windows, macOS, Linux)

- 🗃 Upload to 0x0.st (host file server)

- 🔄 P2P direct file transfer between devices

- 📱 QR Code generation for easy mobile access

---

## 🚀 Usage

```bash
# Share a file directly
wdrop myFile.zip

# Share a folder (it will be zipped automatically)
wdrop myFolder

# If no argument is provided, an interactive menu will appear:
wdrop

# Set default wdrop external upload provider (see below)
wdrop --defaultupload <provierName>
```
[Click to see available providers](providers.txt)

Once started, you'll see an output like:

```
Sharing 'project.zip' at: http://192.168.x.y:z/project.zip
Waiting for connection... Press Ctrl+C to stop.
```

Open the link on another device in the same network to download the file.

A QR code will be generated for easy access from mobile devices. Simply scan the QR code with your smartphone's camera to download the file.

### P2P Mode

When using P2P mode, you can directly transfer files between devices:

```bash
# Set P2P as default upload method
wdrop --defaultupload p2p

# Share a file using P2P
wdrop myFile.zip
```

The P2P mode will provide you with a Peer ID and connection information. You can either:
1. Wait for someone to connect to you
2. Connect to another peer by entering their Peer ID, IP address, and port

A QR code containing the connection information will be generated, making it easy to share your connection details with others.

This allows for direct device-to-device file transfers without an intermediary server.
---

## 📥 Installation

### macOS/linux

```bash
chmod +x install.sh
./install.sh
```

### Windows

Double-click or run from PowerShell:

```powershell
install.bat
```


## 🛠️ Building from source

### Requirements
.NET 8 SDK

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./out
```

Replace `osx-x64` with your [target runtime identifier](https://learn.microsoft.com/pt-br/dotnet/core/rid-catalog).

---

## 🔮 TODO

- [ ] Support for multiple file selection
- [ ] HTML listing of available files (server mode)
- [ ] Receive mode (accept files)
- [ ] Encryption and password protection
- [ ] Direct folder serving without zipping (advanced)
- [ ] External network support via tunneling (e.g. ngrok, Cloudflare Tunnel)
- [ ] Self-destruct mode after download