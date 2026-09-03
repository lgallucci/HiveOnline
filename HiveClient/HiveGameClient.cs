using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;
using HiveNetworking;

namespace HiveClient
{
    public class HiveGameClient : IDisposable
    {
        private readonly string _address;
        private readonly int _port;
        private AsyncTcpClient _tcpClient;
        private readonly LineMessageFramer _messageFramer = new LineMessageFramer();
        private bool disposedValue;

        public event Action<string> MessageReceived;

        public bool IsConnected => _tcpClient?.IsConnected ?? false;

        public HiveGameClient(string address, int port)
        {
            _address = address;
            _port = port;
        }

        public Task Connect() => ConnectAsync();

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _tcpClient = new AsyncTcpClient()
            {
                AutoReconnect = true,
                HostName = _address,
                Port = _port,
                ConnectedCallback = ClientConnected,
                ReceivedCallback = ClientReceived
            };

            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                _tcpClient.AutoReconnect = false;
                _tcpClient.Disconnect();
            });

            await _tcpClient.RunAsync();
        }

        private Task ClientReceived(AsyncTcpClient client, int length)
        {
            var buffer = client.ByteBuffer.Dequeue(length);
            foreach (var message in _messageFramer.Append(buffer))
            {
                Console.WriteLine($"Client Received: {message}");
                MessageReceived?.Invoke(message);
            }

            return Task.CompletedTask;
        }

        private async Task ClientConnected(AsyncTcpClient client, bool isReconnect)
        {
            _messageFramer.Reset();
            Console.WriteLine($"Connected! {(isReconnect ? "isReconnect" : "")}");
            await SendMessage("HELLO");
        }

        public async Task SendMessage(string message)
        {
            if (_tcpClient == null)
                throw new InvalidOperationException("Client is not connected.");

            var bytes = Encoding.UTF8.GetBytes(message + "\n");
            await _tcpClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
        }

        public async Task SendMove(string pieceType, int fromQ, int fromR, int fromS, int toQ, int toR, int toS, string team = "LIGHT")
        {
            var payload = $"MOVE|{team}|{pieceType}|{fromQ},{fromR},{fromS}|{toQ},{toR},{toS}";
            await SendMessage(payload);
        }

        public async Task JoinGame() => await SendMessage("JOIN");

        public void Disconnect()
        {
            _tcpClient?.Disconnect();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tcpClient?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}