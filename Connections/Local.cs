using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Wdrop.Connections;

public class Local
{

    public static async Task UploadLocal(UploadFile uploadFile)
    {
        string localIp = Local.GetLocalIPAddress();
        int port = Local.GetAvailablePort();
        string[] args = [];

        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        var app = builder.Build();

        string rootPathFromFilePath = Path.GetDirectoryName(uploadFile.FilePath);

        WConsole.Info($"Root path from file path: {rootPathFromFilePath}");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPathFromFilePath),
            RequestPath = ""
        });

        string url = $"http://{localIp}:{port}/download/{uploadFile.FileName}";

        app.MapGet("download/{filename}", (string? filename) =>
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = uploadFile.FileName;
            }

            var path = Path.Combine(rootPathFromFilePath, filename);

            if (!System.IO.File.Exists(path))
                return Results.NotFound("Arquivo não encontrado");

            var contentType = "application/octet-stream";
            return Results.File(path, contentType, fileDownloadName: filename);
        });


        var serverTask = app.RunAsync($"http://{localIp}:{port}");

        QRCodeGenerator.GenerateQRCode(url);

        WConsole.Success($"Sharing '{uploadFile.FileName}' to: {url}");
        WConsole.Info("Waiting for connection... Press any key to stop.");
        Console.ReadKey();

        await app.StopAsync();
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