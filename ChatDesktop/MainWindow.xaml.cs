using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatDesktop;

/// <summary>
/// WPF Chat Client - integrates TcpClient logic from Step 1 into the UI.
/// Uses async/await (Step 2) so the UI never freezes while sending/receiving.
/// </summary>
public partial class MainWindow : Window
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _isConnected;
    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
    }

    // =========================================================================
    // CONNECTION
    // =========================================================================

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        if (_isConnected)
        {
            Disconnect();
            return;
        }

        // Validate inputs
        string ip = txtIP.Text.Trim();
        string portText = txtPort.Text.Trim();
        string nickname = txtNickname.Text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            MessageBox.Show("Please enter a server IP.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
        {
            MessageBox.Show("Please enter a valid port (1-65535).", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = $"User_{Random.Shared.Next(1000, 9999)}";
            txtNickname.Text = nickname;
        }

        // Attempt connection
        SetStatus("Connecting...", Brushes.Orange);
        btnConnect.IsEnabled = false;

        try
        {
            _client = new TcpClient();
            _cts = new CancellationTokenSource();

            // Async connect - UI stays responsive
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;

            // Send nickname as first message (same protocol as Console client)
            byte[] nicknameBytes = Encoding.UTF8.GetBytes(nickname);
            await _stream.WriteAsync(nicknameBytes, 0, nicknameBytes.Length);

            // Update UI state
            SetStatus($"Connected to {ip}:{port} as '{nickname}'", Brushes.Green);
            btnConnect.Content = "Disconnect";
            btnConnect.IsEnabled = true;
            txtMessage.IsEnabled = true;
            btnSend.IsEnabled = true;
            txtIP.IsEnabled = false;
            txtPort.IsEnabled = false;
            txtNickname.IsEnabled = false;
            txtMessage.Focus();

            AppendMessage($"[Connected to {ip}:{port}]");

            // Start receiving messages on a background task (Step 2: multi-threading)
            _ = Task.Run(() => ReceiveMessagesAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            SetStatus("Disconnected", Brushes.Gray);
            btnConnect.IsEnabled = true;
            AppendMessage($"[Connection failed: {ex.Message}]");
            _client?.Close();
            _client = null;
        }
    }

    private void Disconnect()
    {
        _isConnected = false;
        _cts?.Cancel();

        try
        {
            _stream?.Close();
            _client?.Close();
        }
        catch { /* ignore cleanup errors */ }

        _stream = null;
        _client = null;

        // Update UI (must run on UI thread)
        Dispatcher.Invoke(() =>
        {
            SetStatus("Disconnected", Brushes.Gray);
            btnConnect.Content = "Connect";
            btnConnect.IsEnabled = true;
            txtMessage.IsEnabled = false;
            btnSend.IsEnabled = false;
            txtIP.IsEnabled = true;
            txtPort.IsEnabled = true;
            txtNickname.IsEnabled = true;
            AppendMessage("[Disconnected]");
        });
    }

    // =========================================================================
    // RECEIVING MESSAGES (runs on background thread)
    // =========================================================================

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (_isConnected && !token.IsCancellationRequested)
            {
                int bytesRead = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0)
                {
                    // Server closed connection
                    Dispatcher.Invoke(() => AppendMessage("[Server disconnected]"));
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Must use Dispatcher to update UI from background thread
                Dispatcher.Invoke(() => AppendMessage(message));
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation during disconnect
        }
        catch (Exception)
        {
            if (_isConnected)
            {
                Dispatcher.Invoke(() => AppendMessage("[Connection lost]"));
            }
        }
        finally
        {
            if (_isConnected)
            {
                Disconnect();
            }
        }
    }

    // =========================================================================
    // SENDING MESSAGES
    // =========================================================================

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void TxtMessage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        if (!_isConnected || _stream == null)
            return;

        string message = txtMessage.Text.Trim();
        if (string.IsNullOrEmpty(message))
            return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);

            // Show own message locally
            AppendMessage($"[You]: {message}");
            txtMessage.Clear();
            txtMessage.Focus();
        }
        catch (Exception ex)
        {
            AppendMessage($"[Send error: {ex.Message}]");
        }
    }

    // =========================================================================
    // UI HELPERS
    // =========================================================================

    private void AppendMessage(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        lstMessages.Items.Add($"[{timestamp}] {message}");

        // Auto-scroll to bottom
        if (lstMessages.Items.Count > 0)
        {
            lstMessages.ScrollIntoView(lstMessages.Items[^1]);
        }
    }

    private void SetStatus(string text, Brush color)
    {
        lblStatus.Text = text;
        lblStatus.Foreground = color;
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isConnected)
        {
            Disconnect();
        }
    }
}
