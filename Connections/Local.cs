using System.Net;
using System.Net.Sockets;

namespace Wdrop.Connections;

public class Local
{

    public static async Task UploadLocal(UploadFile uploadFile)
    {

        string localIp = Local.GetLocalIPAddress();
        int port = Local.GetAvailablePort();
        string url = $"http://{localIp}:{port}/{uploadFile.FileName}";

        WConsole.Success($"Sharing '{uploadFile.FileName}' to: {url}");
        
        QRCodeGenerator.GenerateQRCode(url);

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");
        listener.Start();

        WConsole.Info("Waiting for connection... Press Ctrl+C to stop.");

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.RawUrl == $"/{uploadFile.FileName}")
            {
                WConsole.Info($"Connection from {context.Request.RemoteEndPoint}");

                var response = context.Response;
                response.ContentType = "application/octet-stream";
                response.AddHeader("Content-Disposition", $"attachment; filename={uploadFile.FileName}");
                byte[] buffer = File.ReadAllBytes(uploadFile.FilePath);
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

    }

    public static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static string GetLocalIPAddress()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint?.Address.ToString() ?? "127.0.0.1";
    }


}