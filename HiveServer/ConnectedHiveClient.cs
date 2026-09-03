using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unclassified.Net;
using HiveNetworking;

namespace HiveServer
{
    internal class ConnectedHiveClient
    {
        public Action<ConnectedHiveClient, bool>? Disconnected;
        public Func<ConnectedHiveClient, string, Task>? MessageReceived;

        private readonly AsyncTcpClient tcpClient;
        private readonly LineMessageFramer messageFramer = new LineMessageFramer();

        public Guid Identifier { get; }
        public HiveGame? CurrentGame { get; set; }

        public string RemoteEndpoint => tcpClient?.ServerTcpClient?.Client?.RemoteEndPoint?.ToString() ?? "unknown";

        public ConnectedHiveClient(TcpClient tcpClient)
        {
            Identifier = Guid.NewGuid();
            this.tcpClient = new AsyncTcpClient
            {
                ServerTcpClient = tcpClient,
                ConnectedCallback = ClientConnected,
                ReceivedCallback = MessageReceivedAsync,
                ClosedCallback = ClientClosed
            };
        }

        private async Task ClientConnected(AsyncTcpClient client, bool isReconnected)
        {
            await SendMessage($"WELCOME {Identifier}");
        }

        private async Task MessageReceivedAsync(AsyncTcpClient client, int count)
        {
            var bytes = client.ByteBuffer.Dequeue(count);
            foreach (var message in messageFramer.Append(bytes))
            {
                Console.WriteLine($"Server client {Identifier}: received: {message}");
                if (MessageReceived != null)
                    await MessageReceived(this, message);

                if (message.Equals("bye", StringComparison.OrdinalIgnoreCase))
                    client.Disconnect();
            }
        }

        private void ClientClosed(AsyncTcpClient client, bool remote)
        {
            Disconnected?.Invoke(this, remote);
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var bytes = Encoding.UTF8.GetBytes(message + "\n");
            await tcpClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
        }

        public Task RunAsync()
        {
            return tcpClient.RunAsync();
        }

        public void Disconnect()
        {
            tcpClient.Disconnect();
        }
    }
}