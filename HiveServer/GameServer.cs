using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveServer
{
    public class GameServer : IHostedService
    {
        private List<ConnectedHiveClient> _clients = new List<ConnectedHiveClient>();
        private List<HiveGame> _games = new List<HiveGame>();

        private AsyncTcpListener _listener;

        private Task ClientConnected(TcpClient tcpClient)
        {
            var client = new ConnectedHiveClient(tcpClient);

            client.Disconnected = ClientClosed;

            Console.WriteLine($"Adding Client {client.Identifier} - {tcpClient.Client.RemoteEndPoint}!");
            _clients.Add(client);

            return client.RunAsync();
        }

        private void ClientClosed(ConnectedHiveClient tcpClient, bool closedByRemote)
        {
            Console.WriteLine($"Removing Client {tcpClient.Identifier}! {(closedByRemote ? "closedByRemote" : "" )}");
            _clients.Remove(tcpClient);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            int port = 7777;

            _listener = new AsyncTcpListener
            {
                IPAddress = IPAddress.IPv6Any,
                Port = port,
                ClientConnectedCallback = ClientConnected
            };

            await _listener.RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop(true);

            return Task.CompletedTask;
        }
    }
}
