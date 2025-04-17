using System.Net;
using System.Net.Sockets;

Console.WriteLine(@"
 _    _     _                 
| |  | |   | |                
| |  | | __| |_ __ ___  _ __  
| |/\| |/ _` | '__/ _ \| '_ \ 
\  /\  / (_| | | | (_) | |_) |
 \/  \/ \__,_|_|  \___/| .__/ 
                       | |    
                       |_|     v0.1
         🔗  Wdrop - LAN File Sharing
");

if (args.Length == 0)
{
    Console.WriteLine("Usage: wdrop <file-path>");
    return;
}

string filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found. \n{filePath}");
    return;
}

string fileName = Path.GetFileName(filePath);
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
        byte[] buffer = File.ReadAllBytes(filePath);
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

