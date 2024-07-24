using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RPS_Client
{
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private const int Port = 8888;
        private string _gameMode;
        private StringBuilder _gameHistory = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverAddress = ServerAddressTextBox.Text;
            _client = new TcpClient();

            try
            {
                await _client.ConnectAsync(serverAddress, Port);
                StatusTextBlock.Text = "Connected to server.";

                _gameMode = ((ComboBoxItem)GameModeComboBox.SelectedItem)?.Content.ToString();
                byte[] buffer = Encoding.ASCII.GetBytes(_gameMode);
                await _client.GetStream().WriteAsync(buffer, 0, buffer.Length);

                Task.Run(() => ListenForServerMessages());
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Connection failed: {ex.Message}";
            }
        }

        private async void ChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || !_client.Connected)
            {
                StatusTextBlock.Text = "Not connected to server.";
                return;
            }

            string choice = (string)((Button)sender).Tag;
            byte[] buffer = Encoding.ASCII.GetBytes(choice);

            try
            {
                await _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error sending message: {ex.Message}";
            }
        }

        private async Task ListenForServerMessages()
        {
            var stream = _client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Dispatcher.Invoke(() =>
                {
                    StatusTextBlock.Text = message;
                    _gameHistory.AppendLine(message);
                });
            }

            _client.Close();
            Dispatcher.Invoke(() => StatusTextBlock.Text = "Disconnected from server.");
        }

        private void ShowStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ChoicesTextBlock.Text = _gameHistory.ToString();
        }
    }
}