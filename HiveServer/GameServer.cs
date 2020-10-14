using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveServer
{
    public class GameServer
    {
        private List<ConnectedHiveClient> _clients = new List<ConnectedHiveClient>();
        private List<HiveGame> _games = new List<HiveGame>();

        internal void Run()
        {
            int port = 7777;

            var server = new AsyncTcpListener
            {
                IPAddress = IPAddress.IPv6Any,
                Port = port,
                ClientConnectedCallback = ClientConnected                    
            };
        }

        private Task ClientConnected(TcpClient tcpClient)
        {
            var client = new ConnectedHiveClient(tcpClient);

            client.Disconnected = ClientClosed;

            _clients.Add(client);

            return client.RunAsync();
        }

        private void ClientClosed(ConnectedHiveClient tcpClient, bool closedByRemote)
        {
            _clients.Remove(tcpClient);
        }
    }
}
