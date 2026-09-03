using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveServer
{
    public class GameServer : IHostedService
    {
        private readonly List<ConnectedHiveClient> _clients = new();
        private readonly List<HiveGame> _games = new();
        private readonly Queue<ConnectedHiveClient> _waitingQueue = new();
        private readonly object _syncRoot = new();

        private AsyncTcpListener _listener;
        private readonly int _port;

        public GameServer() : this(GetConfiguredPort())
        {
        }

        public GameServer(int port)
        {
            _port = port;
        }

        private static int GetConfiguredPort()
        {
            var portText = Environment.GetEnvironmentVariable("HIVE_PORT");
            if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var port))
                return port;

            return 7777;
        }

        private async Task ClientConnected(TcpClient tcpClient)
        {
            var client = new ConnectedHiveClient(tcpClient)
            {
                MessageReceived = HandleClientMessage
            };

            client.Disconnected = ClientClosed;

            Console.WriteLine($"Adding Client {client.Identifier} - {tcpClient.Client.RemoteEndPoint}!");
            lock (_syncRoot)
            {
                _clients.Add(client);
            }

            await client.RunAsync();
        }

        private async Task HandleClientMessage(ConnectedHiveClient client, string message)
        {
            var normalized = message.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            if (normalized.StartsWith("MOVE|", StringComparison.OrdinalIgnoreCase))
            {
                await RouteMove(client, normalized);
                return;
            }

            switch (normalized.ToUpperInvariant())
            {
                case "HELLO":
                    await client.SendMessage($"SERVER_HELLO {client.Identifier}");
                    break;
                case "PING":
                    await client.SendMessage("PONG");
                    break;
                case "JOIN":
                    await QueueForMatch(client);
                    break;
                case "BYE":
                    client.Disconnect();
                    break;
                default:
                    await client.SendMessage($"UNKNOWN_COMMAND:{normalized}");
                    break;
            }
        }

        private async Task QueueForMatch(ConnectedHiveClient client)
        {
            ConnectedHiveClient playerOne = null;
            ConnectedHiveClient playerTwo = null;
            HiveGame game = null;
            var alreadyQueued = false;
            var alreadyInGame = false;

            lock (_syncRoot)
            {
                alreadyInGame = client.CurrentGame != null;
                alreadyQueued = !alreadyInGame && _waitingQueue.Contains(client);

                if (!alreadyInGame && !alreadyQueued)
                    _waitingQueue.Enqueue(client);

                if (!alreadyInGame && !alreadyQueued && _waitingQueue.Count >= 2)
                {
                    playerOne = _waitingQueue.Dequeue();
                    playerTwo = _waitingQueue.Dequeue();
                    game = new HiveGame(playerOne, playerTwo);
                    playerOne.CurrentGame = game;
                    playerTwo.CurrentGame = game;
                    _games.Add(game);
                }
            }

            if (alreadyInGame || alreadyQueued)
            {
                await client.SendMessage(alreadyInGame ? "ERROR ALREADY_IN_GAME" : "ERROR ALREADY_WAITING");
                return;
            }

            if (playerOne == null || playerTwo == null)
            {
                await client.SendMessage("WAITING_FOR_OPPONENT");
                return;
            }

            await playerOne.SendMessage($"MATCHED {playerOne.Identifier} {playerTwo.Identifier}");
            await playerTwo.SendMessage($"MATCHED {playerTwo.Identifier} {playerOne.Identifier}");
            await playerOne.SendMessage("YOU_ARE_PLAYER_1");
            await playerTwo.SendMessage("YOU_ARE_PLAYER_2");

            Console.WriteLine($"Matched players in game {game.Id}");
        }

        private async Task RouteMove(ConnectedHiveClient client, string message)
        {
            ConnectedHiveClient opponent = null;
            lock (_syncRoot)
            {
                var game = client.CurrentGame;
                if (game != null)
                    opponent = game.GetOpponent(client);
            }

            if (opponent == null)
            {
                await client.SendMessage("ERROR NOT_IN_GAME");
                return;
            }

            await opponent.SendMessage(message);
        }

        private void ClientClosed(ConnectedHiveClient tcpClient, bool closedByRemote)
        {
            Console.WriteLine($"Removing Client {tcpClient.Identifier}! {(closedByRemote ? "closedByRemote" : "")}");
            ConnectedHiveClient opponent = null;

            lock (_syncRoot)
            {
                _clients.Remove(tcpClient);
                if (_waitingQueue.Contains(tcpClient))
                {
                    var remaining = new Queue<ConnectedHiveClient>();
                    while (_waitingQueue.Count > 0)
                    {
                        var queuedClient = _waitingQueue.Dequeue();
                        if (!ReferenceEquals(queuedClient, tcpClient))
                            remaining.Enqueue(queuedClient);
                    }
                    _waitingQueue.Clear();
                    while (remaining.Count > 0)
                        _waitingQueue.Enqueue(remaining.Dequeue());
                }

                if (tcpClient.CurrentGame != null)
                {
                    var game = tcpClient.CurrentGame;
                    opponent = game.GetOpponent(tcpClient);
                    _games.Remove(game);
                    tcpClient.CurrentGame = null;
                    opponent.CurrentGame = null;
                }
            }

            if (opponent != null)
                _ = opponent.SendMessage("OPPONENT_DISCONNECTED");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new AsyncTcpListener
            {
                IPAddress = IPAddress.IPv6Any,
                Port = _port,
                ClientConnectedCallback = ClientConnected
            };

            Console.WriteLine($"Hive server listening on port {_port}");
            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                try
                {
                    _listener?.Stop(true);
                }
                catch (InvalidOperationException)
                {
                }
            });

            await _listener.RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _listener?.Stop(true);
            }
            catch (InvalidOperationException)
            {
            }
            return Task.CompletedTask;
        }
    }
}
