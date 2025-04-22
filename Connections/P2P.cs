using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Sharprompt;

namespace Wdrop.Connections;

public class P2P
{
    public static async Task<int> StartP2PServer(UploadFile uploadFile)
    {
        string localIp = Local.GetLocalIPAddress();
        int port = Local.GetAvailablePort();
        string peerId = Guid.NewGuid().ToString().Substring(0, 8);

        Console.WriteLine($"Starting P2P server for '{uploadFile.FileName}'...");
        Console.WriteLine($"Your Peer ID: {peerId}");

        var tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        Console.WriteLine($"P2P server started on {localIp}:{port}");
        
        string connectionInfo = $"P2P:{peerId}:{localIp}:{port}";
        QRCodeGenerator.GenerateQRCode(connectionInfo);
        
        Console.WriteLine("\nWaiting for peer connections... Press Ctrl+C to stop.");

        var listenTask = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine($"Peer connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                    _ = Task.Run(() => HandleP2PClient(client, uploadFile.FilePath, uploadFile.FileName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting connection: {ex.Message}");
                }
            }
        });

        Console.WriteLine("\nOptions:");
        Console.WriteLine("1. Wait for someone to connect to you");
        Console.WriteLine("2. Connect to another peer");

        string choice = Prompt.Select("What would you like to do?", new[] { "Wait", "Connect to peer" });

        if (choice == "Connect to peer")
        {
            string remotePeerId = Prompt.Input<string>("Enter the remote Peer ID");
            string remoteIp = Prompt.Input<string>("Enter the remote IP address");
            string remotePortStr = Prompt.Input<string>("Enter the remote port");

            if (int.TryParse(remotePortStr, out int remotePort))
            {
                await ConnectToP2PPeer(remoteIp, remotePort, uploadFile.FilePath, uploadFile.FileName);
            }
            else
            {
                Console.WriteLine("Invalid port number.");
            }
        }

        await listenTask;

        return (int)decimal.Zero;
    }

    static async Task HandleP2PClient(TcpClient client, string filePath, string fileName)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var fileStream = File.OpenRead(filePath))
            {
                var fileInfo = new
                {
                    FileName = fileName,
                    FileSize = fileStream.Length
                };

                byte[] fileInfoBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fileInfo));
                byte[] fileInfoLengthBytes = BitConverter.GetBytes(fileInfoBytes.Length);

                await stream.WriteAsync(fileInfoLengthBytes, 0, fileInfoLengthBytes.Length);
                await stream.WriteAsync(fileInfoBytes, 0, fileInfoBytes.Length);

                byte[] buffer = new byte[8192];
                long totalBytesSent = 0;
                int bytesRead;

                Console.WriteLine($"Starting file transfer of {fileName} ({fileStream.Length} bytes)...");

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesSent += bytesRead;

                    // Show progress
                    double progress = (double)totalBytesSent / fileStream.Length * 100;
                    Console.Write($"\rProgress: {progress:F2}% ({totalBytesSent}/{fileStream.Length} bytes)    ");
                }

                Console.WriteLine("\nFile transfer completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError during file transfer: {ex.Message}");
        }
    }

    static async Task ConnectToP2PPeer(string remoteIp, int remotePort, string filePath, string fileName)
    {
        Console.WriteLine($"Connecting to peer at {remoteIp}:{remotePort}...");

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(remoteIp, remotePort);

            Console.WriteLine("Connected to peer. Waiting to receive file...");

            using var stream = client.GetStream();

            byte[] fileInfoLengthBytes = new byte[4];
            await stream.ReadAsync(fileInfoLengthBytes, 0, fileInfoLengthBytes.Length);
            int fileInfoLength = BitConverter.ToInt32(fileInfoLengthBytes, 0);

            byte[] fileInfoBytes = new byte[fileInfoLength];
            await stream.ReadAsync(fileInfoBytes, 0, fileInfoBytes.Length);

            string fileInfoJson = System.Text.Encoding.UTF8.GetString(fileInfoBytes);
            var fileInfo = JsonSerializer.Deserialize<P2PFileInfo>(fileInfoJson);

            Console.WriteLine($"Receiving file: {fileInfo.FileName} ({fileInfo.FileSize} bytes)");

            string downloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }

            string savePath = Path.Combine(downloadDir, fileInfo.FileName);

            using var fileStream = File.Create(savePath);
            byte[] buffer = new byte[8192];
            long totalBytesReceived = 0;
            int bytesRead;

            while (totalBytesReceived < fileInfo.FileSize &&
                   (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesReceived += bytesRead;

                double progress = (double)totalBytesReceived / fileInfo.FileSize * 100;
                Console.Write($"\rProgress: {progress:F2}% ({totalBytesReceived}/{fileInfo.FileSize} bytes)    ");
            }

            Console.WriteLine("\nFile received successfully!");
            Console.WriteLine($"Saved to: {savePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to peer: {ex.Message}");
        }
    }

    class P2PFileInfo
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}