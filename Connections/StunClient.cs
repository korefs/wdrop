using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Wdrop.Connections;

/// <summary>
/// A simple STUN client implementation to discover public IP address and port
/// Based on RFC 5389 (STUN protocol)
/// </summary>
public class StunClient
{
    // STUN message types
    private const ushort BindingRequest = 0x0001;
    private const ushort BindingResponse = 0x0101;
    
    // STUN attributes
    private const ushort MappedAddress = 0x0001;
    private const ushort XorMappedAddress = 0x0020;
    
    // Well-known public STUN servers
    private static readonly string[] StunServers = new[]
    {
        "stun.l.google.com:19302",
        "stun1.l.google.com:19302",
        "stun2.l.google.com:19302",
        "stun.ekiga.net:3478",
        "stun.ideasip.com:3478",
        "stun.stunprotocol.org:3478"
    };
    
    /// <summary>
    /// Discovers the public IP address and port using STUN protocol
    /// </summary>
    /// <param name="localPort">The local port to bind to</param>
    /// <returns>The public endpoint (IP:port) or null if discovery fails</returns>
    public static async Task<IPEndPoint> DiscoverPublicEndpoint(int localPort)
    {
        using var udpClient = new UdpClient(localPort);
        udpClient.Client.ReceiveTimeout = 5000; // 5 seconds timeout
        
        foreach (var stunServer in StunServers)
        {
            try
            {
                var parts = stunServer.Split(':');
                var host = parts[0];
                var port = int.Parse(parts[1]);
                
                WConsole.Info($"Trying STUN server: {stunServer}...");
                
                // Resolve the STUN server hostname
                var addresses = await Dns.GetHostAddressesAsync(host);
                if (addresses.Length == 0) continue;
                
                var serverEndpoint = new IPEndPoint(addresses[0], port);
                
                // Create STUN binding request
                byte[] request = CreateStunBindingRequest();
                
                // Send the request
                await udpClient.SendAsync(request, request.Length, serverEndpoint);
                
                // Receive the response
                var result = await udpClient.ReceiveAsync();
                byte[] response = result.Buffer;
                
                // Parse the response
                var publicEndpoint = ParseStunResponse(response);
                if (publicEndpoint != null)
                {
                    WConsole.Success($"Public endpoint discovered: {publicEndpoint}");
                    return publicEndpoint;
                }
            }
            catch (Exception ex)
            {
                WConsole.Warn($"Failed to use STUN server {stunServer}: {ex.Message}");
                // Continue with the next server
            }
        }
        
        WConsole.Warn("Failed to discover public endpoint using STUN. Falling back to local IP.");
        return null;
    }
    
    /// <summary>
    /// Creates a STUN binding request message
    /// </summary>
    private static byte[] CreateStunBindingRequest()
    {
        var request = new byte[20]; // STUN header is 20 bytes
        
        // Message type: Binding request
        request[0] = 0x00;
        request[1] = 0x01;
        
        // Message length: 0 bytes (no attributes)
        request[2] = 0x00;
        request[3] = 0x00;
        
        // Magic cookie: 0x2112A442 in network byte order
        request[4] = 0x21;
        request[5] = 0x12;
        request[6] = 0xA4;
        request[7] = 0x42;
        
        // Transaction ID: 12 random bytes
        var random = new Random();
        for (int i = 8; i < 20; i++)
        {
            request[i] = (byte)random.Next(256);
        }
        
        return request;
    }
    
    /// <summary>
    /// Parses a STUN response to extract the mapped address (public IP and port)
    /// </summary>
    private static IPEndPoint ParseStunResponse(byte[] response)
    {
        // Check if this is a STUN binding response
        if (response.Length < 20) return null;
        
        ushort messageType = (ushort)((response[0] << 8) | response[1]);
        if (messageType != BindingResponse) return null;
        
        ushort messageLength = (ushort)((response[2] << 8) | response[3]);
        if (messageLength < 4) return null;
        
        // Look for XOR-MAPPED-ADDRESS or MAPPED-ADDRESS attribute
        int index = 20; // Start after the header
        while (index < response.Length - 4)
        {
            ushort attributeType = (ushort)((response[index] << 8) | response[index + 1]);
            ushort attributeLength = (ushort)((response[index + 2] << 8) | response[index + 3]);
            
            if (attributeType == XorMappedAddress || attributeType == MappedAddress)
            {
                if (attributeLength < 8) break; // Not enough data
                
                // Skip the attribute header (4 bytes) and the first 2 bytes of value (family and reserved)
                index += 6;
                
                // Read port (2 bytes) and IP address (4 bytes for IPv4)
                ushort port = (ushort)((response[index] << 8) | response[index + 1]);
                byte[] ipBytes = new byte[4];
                Array.Copy(response, index + 2, ipBytes, 0, 4);
                
                // If this is XOR-MAPPED-ADDRESS, we need to XOR with the magic cookie
                if (attributeType == XorMappedAddress)
                {
                    port ^= 0x2112; // XOR with first 2 bytes of magic cookie
                    
                    // XOR IP with magic cookie
                    ipBytes[0] ^= 0x21;
                    ipBytes[1] ^= 0x12;
                    ipBytes[2] ^= 0xA4;
                    ipBytes[3] ^= 0x42;
                }
                
                var ip = new IPAddress(ipBytes);
                return new IPEndPoint(ip, port);
            }
            
            // Move to the next attribute
            index += 4 + attributeLength;
            // Attributes are padded to 4-byte boundaries
            if (attributeLength % 4 != 0)
            {
                index += 4 - (attributeLength % 4);
            }
        }
        
        return null;
    }
}