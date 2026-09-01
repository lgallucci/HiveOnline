using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveClient
{
    public class HiveGameClient : IDisposable
    {
        private readonly string _address;
        private readonly int _port;
        private readonly Queue<string> _receivedMessages = new Queue<string>();
        private AsyncTcpClient _tcpClient;
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

            await _tcpClient.RunAsync();
        }

        private Task ClientReceived(AsyncTcpClient client, int length)
        {
            var buffer = client.ByteBuffer.Dequeue(length);
            var message = Encoding.UTF8.GetString(buffer, 0, length).Trim();

            if (!string.IsNullOrWhiteSpace(message))
            {
                lock (_receivedMessages)
                {
                    _receivedMessages.Enqueue(message);
                }
            }

            Console.WriteLine($"Client Received: {message}");
            MessageReceived?.Invoke(message);

            return Task.CompletedTask;
        }

        public bool TryDequeueMessage(out string message)
        {
            lock (_receivedMessages)
            {
                if (_receivedMessages.Count > 0)
                {
                    message = _receivedMessages.Dequeue();
                    return true;
                }
            }

            message = string.Empty;
            return false;
        }

        private async Task ClientConnected(AsyncTcpClient client, bool isReconnect)
        {
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