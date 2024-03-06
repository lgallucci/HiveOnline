using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveServer
{
    internal class ConnectedHiveClient
    {
        public Action<ConnectedHiveClient, bool> Disconnected;

        private AsyncTcpClient tcpClient;

        public Guid Identifier { get; internal set; }

        public ConnectedHiveClient(TcpClient tcpClient)
        {
            Identifier = Guid.NewGuid();
            this.tcpClient = new AsyncTcpClient
            {
                ServerTcpClient = tcpClient,
                ConnectedCallback = ClientConnected,
                ReceivedCallback = MessageRecieved,
                ClosedCallback = ClientClosed
            };
        }

        private async Task ClientConnected(AsyncTcpClient client, bool isReconnected)
        {
            await Task.Delay(500);
            byte[] bytes = Encoding.UTF8.GetBytes($"Hello, {client.ServerTcpClient.Client.RemoteEndPoint}, my name is Server. Talk to me.");
            await client.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
        }

        private async Task MessageRecieved(AsyncTcpClient client, int count)
        {
            byte[] bytes = client.ByteBuffer.Dequeue(count);
            string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($"Server client {Identifier}: received: {message}");

            bytes = Encoding.UTF8.GetBytes("You said: " + message);
            await client.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

            if (message == "bye")
            {
                // Let the server close the connection
                client.Disconnect();
            }
        }

        private void ClientClosed(AsyncTcpClient client, bool remote)
        {
            Disconnected?.Invoke(this, remote);
        }

        public Task RunAsync()
        {
            return tcpClient.RunAsync();
        }
    }
}