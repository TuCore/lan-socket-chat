using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer;

class Program
{
    // Thread-safe dictionary to track all connected clients
    private static readonly ConcurrentDictionary<string, TcpClient> _clients = new();
    private static readonly object _lock = new();

    static async Task Main(string[] args)
    {
        int port = 9000;
        if (args.Length > 0 && int.TryParse(args[0], out int customPort))
            port = customPort;

        // Step 1: Create TcpListener to listen on all interfaces
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine("=== CHAT SERVER ===");
        Console.WriteLine($"Server started on port {port}");
        Console.WriteLine("Waiting for clients...");
        Console.WriteLine();

        try
        {
            // Step 2: Accept clients in a loop (async) - each client gets its own Task
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                string clientId = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
                _clients.TryAdd(clientId, client);

                Console.WriteLine($"[+] Client connected: {clientId} (Total: {_clients.Count})");

                // Each client is handled in a separate Task (multi-threading)
                _ = Task.Run(() => HandleClientAsync(clientId, client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Handles a single client connection: reads messages and broadcasts to all others.
    /// Runs on its own thread/task so multiple clients work simultaneously.
    /// </summary>
    private static async Task HandleClientAsync(string clientId, TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        string? nickname = null;

        try
        {
            // First message from client is their nickname
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                nickname = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"[*] {clientId} set nickname: {nickname}");
                await BroadcastAsync($"[Server] {nickname} has joined the chat!", clientId);
            }

            // Continuously read messages from this client
            while (client.Connected)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break; // Client disconnected

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (string.IsNullOrEmpty(message))
                    continue;

                string displayName = nickname ?? clientId;
                string fullMessage = $"{displayName}: {message}";

                Console.WriteLine($"[MSG] {fullMessage}");

                // Broadcast this message to ALL other connected clients
                await BroadcastAsync(fullMessage, clientId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error with {nickname ?? clientId}: {ex.Message}");
        }
        finally
        {
            // Cleanup when client disconnects
            _clients.TryRemove(clientId, out _);
            client.Close();
            string displayName = nickname ?? clientId;
            Console.WriteLine($"[-] {displayName} disconnected. (Total: {_clients.Count})");
            await BroadcastAsync($"[Server] {displayName} has left the chat.", clientId);
        }
    }

    /// <summary>
    /// Sends a message to all connected clients except the sender.
    /// </summary>
    private static async Task BroadcastAsync(string message, string excludeClientId)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var kvp in _clients)
        {
            if (kvp.Key == excludeClientId)
                continue;

            try
            {
                if (kvp.Value.Connected)
                {
                    NetworkStream stream = kvp.Value.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch
            {
                // Client might have disconnected; ignore and continue
            }
        }
    }
}
