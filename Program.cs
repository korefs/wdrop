using System.Net;
using System.Net.Sockets;
using Sharprompt;
using Wdrop;

Console.WriteLine(@"
 _    _     _                 
| |  | |   | |                
| |  | | __| |_ __ ___  _ __  
| |/\| |/ _` | '__/ _ \| '_ \ 
\  /\  / (_| | | | (_) | |_) |
 \/  \/ \__,_|_|  \___/| .__/ 
                       | |    
                       |_|     v0.2
         🔗  Wdrop - LAN File Sharing
");

string? pathToShare;
bool isDirectory = false;

var config = Config.LoadConfig();

if (args[0] == "--defaultupload")
{
    // todo: implement default upload

    if (args.Length < 2)
    {
        Console.WriteLine($"Current Default upload destination is '{config["uploadto"]}'");
        return;
    }

    Config.SetConfig("uploadto", args[1]);
    Console.WriteLine($"✅ Default upload destination set to '{args[1]}'");
    return;
}

if (args.Length == 0)
{
    Console.WriteLine("📂 No file was passed as an argument. Selecting via menu...");

    string[] items = [.. Directory.GetFiles(Directory.GetCurrentDirectory())];

    if (items.Length == 0)
    {
        Console.WriteLine("❌ No files found in current folder.");
        return;
    }

    string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory());

    items = [.. items, .. directories];

    string selectedFile = Prompt.Select("Choose a file or folder to share", [.. items.Select(Path.GetFileName)]);

    pathToShare = Path.Combine(Directory.GetCurrentDirectory(), selectedFile);
    isDirectory = Directory.Exists(pathToShare);
}
else
{
    pathToShare = args[0];
    isDirectory = Directory.Exists(pathToShare);
}


if (isDirectory)
{
    Console.WriteLine("Creating .zip from folder...");
    string zipPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(pathToShare) + ".zip");
    if (File.Exists(zipPath)) File.Delete(zipPath);
    System.IO.Compression.ZipFile.CreateFromDirectory(pathToShare, zipPath);
    pathToShare = zipPath;
}

if (!File.Exists(pathToShare))
{
    Console.WriteLine($"File not found. \n{pathToShare}");
    return;
}

string fileName = Path.GetFileName(pathToShare);
string localIp = GetLocalIPAddress();
int port = GetAvailablePort();
string url = $"http://{localIp}:{port}/{fileName}";

Console.WriteLine($"Sharing '{fileName}' to: {url}");

using var listener = new HttpListener();
listener.Prefixes.Add($"http://+:{port}/");
listener.Start();

Console.WriteLine("Waiting for connection... Press Ctrl+C to stop.");

while (true)
{
    var context = listener.GetContext();
    if (context.Request.RawUrl == $"/{fileName}")
    {
        Console.WriteLine($"Connection from {context.Request.RemoteEndPoint}");

        var response = context.Response;
        response.ContentType = "application/octet-stream";
        response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
        byte[] buffer = File.ReadAllBytes(pathToShare);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
    else
    {
        context.Response.StatusCode = 404;
        context.Response.Close();
    }
}

static int GetAvailablePort()
{
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

static string GetLocalIPAddress()
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    socket.Connect("8.8.8.8", 65530);
    var endPoint = socket.LocalEndPoint as IPEndPoint;
    return endPoint?.Address.ToString() ?? "127.0.0.1";
}

