using System.Net.Sockets;
using System.Text;

namespace ChatConsole;

class Program
{
    private static bool _isConnected = false;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CHAT CLIENT ===");
        Console.WriteLine();

        // Get server IP
        Console.Write("Enter server IP (default 127.0.0.1): ");
        string? ip = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(ip))
            ip = "127.0.0.1";

        // Get server port
        Console.Write("Enter server port (default 9000): ");
        string? portInput = Console.ReadLine()?.Trim();
        int port = 9000;
        if (!string.IsNullOrEmpty(portInput) && int.TryParse(portInput, out int customPort))
            port = customPort;

        // Get nickname
        Console.Write("Enter your nickname: ");
        string? nickname = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(nickname))
            nickname = $"User_{Random.Shared.Next(1000, 9999)}";

        Console.WriteLine();
        Console.WriteLine($"Connecting to {ip}:{port} as '{nickname}'...");

        TcpClient client = new TcpClient();

        try
        {
            // Step 1: Connect using TcpClient
            await client.ConnectAsync(ip, port);
            _isConnected = true;
            NetworkStream stream = client.GetStream();

            Console.WriteLine("Connected! Type messages and press Enter to send.");
            Console.WriteLine("Type /quit to disconnect.");
            Console.WriteLine(new string('-', 50));

            // Send nickname as the first message
            byte[] nicknameBytes = Encoding.UTF8.GetBytes(nickname);
            await stream.WriteAsync(nicknameBytes, 0, nicknameBytes.Length);

            // Step 2: Start a background Task to receive messages (multi-threading)
            // This runs on a separate thread so we can read AND write simultaneously
            Task receiveTask = Task.Run(() => ReceiveMessagesAsync(stream));

            // Main thread handles sending messages
            while (_isConnected)
            {
                string? input = Console.ReadLine();
                if (input == null)
                    continue;

                if (input.Trim().Equals("/quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Disconnecting...");
                    break;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                input = ReplaceEmojis(input);

                // Limit message length to prevent spam/infinite text
                if (input.Length > 500)
                {
                    Console.WriteLine("[System] Message too long. Truncating to 500 characters.");
                    input = input.Substring(0, 500);
                }

                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(input);
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error sending] {ex.Message}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
        finally
        {
            _isConnected = false;
            client.Close();
            Console.WriteLine("Disconnected. Press any key to exit.");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Continuously reads incoming messages from the server on a background thread.
    /// </summary>
    private static async Task ReceiveMessagesAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (_isConnected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine("[Server disconnected]");
                    _isConnected = false;
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
        }
        catch (Exception)
        {
            if (_isConnected)
            {
                Console.WriteLine("[Connection lost]");
                _isConnected = false;
            }
        }
    }

    private static string ReplaceEmojis(string text)
    {
        return text.Replace(":)", "😊")
                   .Replace(":(", "😢")
                   .Replace(":D", "😀")
                   .Replace(";)", "😉")
                   .Replace("<3", "❤️")
                   .Replace("(y)", "👍")
                   .Replace(":O", "😲");
    }
}
