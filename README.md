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

---

## 🚀 Usage

```bash
# Share a file directly
wdrop myFile.zip

# Share a folder (it will be zipped automatically)
wdrop myFolder

# If no argument is provided, an interactive menu will appear:
wdrop
```

Once started, you'll see an output like:

```
Sharing 'project.zip' at: http://192.168.x.y:z/project.zip
Waiting for connection... Press Ctrl+C to stop.
```

Open the link on another device in the same network to download the file.

---

## 📥 Installation

### macOS

```bash
chmod +x install-macos.sh
./install-macos.sh
```

### Windows

Double-click or run from PowerShell:

```powershell
install-win.bat
```


## 🛠️ Building from source

### Requirements
.NET 8 SDK

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./out
```

Replace `osx-x64` with your target runtime identifier.

---

## 🔮 TODO

- [ ] Support for multiple file selection
- [ ] HTML listing of available files (server mode)
- [ ] QR Code generation for easy mobile access
- [ ] Add upload possibility to 0x0.st
- [ ] Drag-and-drop GUI version
- [ ] Receive mode (accept files)
- [ ] Create a client to support P2P connection
- [ ] Encryption and password protection
- [ ] Direct folder serving without zipping (advanced)
- [ ] External network support via tunneling (e.g. ngrok, Cloudflare Tunnel)
- [ ] Self-destruct mode after download