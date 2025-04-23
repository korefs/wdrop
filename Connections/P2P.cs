using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Sharprompt;

namespace Wdrop.Connections;

public class P2P
{
    // Connection info structure
    private class ConnectionInfo
    {   
        public string PeerId { get; set; }
        public string LocalIp { get; set; }
        public string PublicIp { get; set; }
        public int Port { get; set; }
        public bool HasPublicIp => !string.IsNullOrEmpty(PublicIp);
        
        public string GetConnectionString()
        {
            // Use public IP if available, otherwise use local IP
            string ip = HasPublicIp ? PublicIp : LocalIp;
            return $"P2P:{PeerId}:{ip}:{Port}";
        }
    }
    public static async Task StartP2PServer(UploadFile uploadFile)
    {
        string localIp = Local.GetLocalIPAddress();
        int port = Local.GetAvailablePort();
        string peerId = Guid.NewGuid().ToString().Substring(0, 8);

        WConsole.Info($"Starting P2P server for '{uploadFile.FileName}'...");
        WConsole.Info($"Your Peer ID: {peerId}");

        // Create connection info object
        var connectionInfo = new ConnectionInfo
        {
            PeerId = peerId,
            LocalIp = localIp,
            Port = port
        };
        
        // Start TCP listener
        var tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        
        WConsole.Info($"P2P server started on local address {localIp}:{port}");
        
        // Try to discover public IP and port using STUN
        try
        {
            WConsole.Info("Discovering public IP address and port...");
            var publicEndpoint = await StunClient.DiscoverPublicEndpoint(port);
            
            if (publicEndpoint != null)
            {
                connectionInfo.PublicIp = publicEndpoint.Address.ToString();
                WConsole.Success($"Public IP address discovered: {connectionInfo.PublicIp}");
                WConsole.Info("Note: For P2P connections across different networks, you may need to configure port forwarding on your router.");
            }
        }
        catch (Exception ex)
        {
            WConsole.Warn($"Failed to discover public IP: {ex.Message}");
            WConsole.Warn("Falling back to local IP address only.");
        }
        
        // Generate connection string and QR code
        string connectionString = connectionInfo.GetConnectionString();
        QRCodeGenerator.GenerateQRCode(connectionString);
        
        if (connectionInfo.HasPublicIp)
        {
            WConsole.Info($"Share this connection info with your peer: {connectionString}");
        }
        else
        {
            WConsole.Warn("No public IP address discovered. P2P will only work on the same local network.");
            WConsole.Warn("For connections across different networks, you'll need to use port forwarding or a relay server.");
        }
        
        WConsole.Info("\nWaiting for peer connections... Press Ctrl+C to stop.");

        var listenTask = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var client = await tcpListener.AcceptTcpClientAsync();
                    WConsole.Info($"Peer connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                    _ = Task.Run(() => HandleP2PClient(client, uploadFile.FilePath, uploadFile.FileName));
                }
                catch (Exception ex)
                {
                    WConsole.Info($"Error accepting connection: {ex.Message}");
                }
            }
        });

        WConsole.Info("\nOptions:");
        WConsole.Info("1. Wait for someone to connect to you");
        WConsole.Info("2. Connect to another peer");

        string choice = Prompt.Select("What would you like to do?", new[] { "Wait", "Connect to peer" });

        if (choice == "Connect to peer")
        {
            string remotePeerId = Prompt.Input<string>("Enter the remote Peer ID");
            string remoteIp = Prompt.Input<string>("Enter the remote IP address");
            string remotePortStr = Prompt.Input<string>("Enter the remote port");

            if (int.TryParse(remotePortStr, out int remotePort))
            {
                try
                {
                    WConsole.Info($"Attempting to connect to peer at {remoteIp}:{remotePort}...");
                    
                    // Try to resolve hostname if it's not an IP address
                    IPAddress ipAddress;
                    if (!IPAddress.TryParse(remoteIp, out ipAddress))
                    {
                        WConsole.Info($"Resolving hostname {remoteIp}...");
                        var addresses = await Dns.GetHostAddressesAsync(remoteIp);
                        if (addresses.Length > 0)
                        {
                            ipAddress = addresses[0];
                            WConsole.Info($"Resolved to IP address: {ipAddress}");
                        }
                        else
                        {
                            WConsole.Error($"Could not resolve hostname {remoteIp}");
                            return;
                        }
                    }
                    
                    await ConnectToP2PPeer(ipAddress.ToString(), remotePort, uploadFile.FilePath, uploadFile.FileName);
                }
                catch (Exception ex)
                {
                    WConsole.Error($"Error connecting to peer: {ex.Message}");
                }
            }
            else
            {
                WConsole.Error("Invalid port number.");
            }
        }

        await listenTask;
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

                WConsole.Info($"Starting file transfer of {fileName} ({fileStream.Length} bytes)...");

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesSent += bytesRead;

                    // Show progress
                    double progress = (double)totalBytesSent / fileStream.Length * 100;
                    Console.Write($"\rProgress: {progress:F2}% ({totalBytesSent}/{fileStream.Length} bytes)    ");
                }

                WConsole.Success("\nFile transfer completed successfully!");
            }
        }
        catch (Exception ex)
        {
            WConsole.Error($"\nError during file transfer: {ex.Message}");
        }
    }

    static async Task ConnectToP2PPeer(string remoteIp, int remotePort, string filePath, string fileName)
    {
        WConsole.Info($"Connecting to peer at {remoteIp}:{remotePort}...");

        try
        {
            using var client = new TcpClient();
            
            // Set connection timeout
            var connectTask = client.ConnectAsync(remoteIp, remotePort);
            var timeoutTask = Task.Delay(10000); // 10 seconds timeout
            
            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                throw new TimeoutException("Connection attempt timed out. The peer might be behind a firewall or NAT.");
            }
            
            // Connection successful
            WConsole.Success("Connected to peer. Waiting to receive file...");

            using var stream = client.GetStream();

            byte[] fileInfoLengthBytes = new byte[4];
            await stream.ReadAsync(fileInfoLengthBytes, 0, fileInfoLengthBytes.Length);
            int fileInfoLength = BitConverter.ToInt32(fileInfoLengthBytes, 0);

            byte[] fileInfoBytes = new byte[fileInfoLength];
            await stream.ReadAsync(fileInfoBytes, 0, fileInfoBytes.Length);

            string fileInfoJson = System.Text.Encoding.UTF8.GetString(fileInfoBytes);
            var fileInfo = JsonSerializer.Deserialize<P2PFileInfo>(fileInfoJson);

            WConsole.Info($"Receiving file: {fileInfo.FileName} ({fileInfo.FileSize} bytes)");

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

            WConsole.Success("\nFile received successfully!");
            WConsole.Info($"Saved to: {savePath}");
        }
        catch (Exception ex)
        {
            WConsole.Error($"Error connecting to peer: {ex.Message}");
            WConsole.Info("Possible reasons for connection failure:");
            WConsole.Info("1. The peer is behind a NAT/firewall without port forwarding configured");
            WConsole.Info("2. The IP address or port number is incorrect");
            WConsole.Info("3. The peer's P2P server is not running");
        }
    }



    class P2PFileInfo
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}