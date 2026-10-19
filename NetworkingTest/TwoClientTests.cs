using System.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using HiveClient;
using HiveServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkingTest;

[TestClass]
public class TwoClientTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private GameServer _server = null!;
    private CancellationTokenSource _cts = null!;
    private Task _serverTask = null!;
    private int _port;
    private readonly List<TestClient> _clients = new();

    [TestInitialize]
    public async Task Start()
    {
        _port = GetFreePort();
        _server = new GameServer(_port);
        _cts = new CancellationTokenSource();
        _serverTask = Task.Run(() => _server.StartAsync(_cts.Token));
        await Task.Delay(300);
    }

    [TestCleanup]
    public async Task Stop()
    {
        foreach (var c in _clients)
            c.Dispose();
        _cts.Cancel();
        await _server.StopAsync(CancellationToken.None);
        await Task.WhenAny(_serverTask, Task.Delay(2000));
    }

    [TestMethod]
    public async Task ClientReceivesWelcomeAndHello()
    {
        var a = await Connect();
        await a.WaitFor(m => m.StartsWith("SERVER_HELLO "));
    }

    [TestMethod]
    public async Task SingleClientJoinWaitsForOpponent()
    {
        var a = await Connect();
        await a.Client.JoinGame();
        await a.WaitFor(m => m == "WAITING_FOR_OPPONENT");
    }

    [TestMethod]
    public async Task TwoClientsJoiningAreMatched()
    {
        var a = await Connect();
        var b = await Connect();

        await a.Client.JoinGame();
        await a.WaitFor(m => m == "WAITING_FOR_OPPONENT");
        await b.Client.JoinGame();

        var matchedA = await a.WaitFor(m => m.StartsWith("MATCHED "));
        var matchedB = await b.WaitFor(m => m.StartsWith("MATCHED "));
        await a.WaitFor(m => m == "YOU_ARE_PLAYER_1");
        await b.WaitFor(m => m == "YOU_ARE_PLAYER_2");

        var idsA = matchedA.Split(' ');
        var idsB = matchedB.Split(' ');
        Assert.AreEqual(idsA[1], idsB[2]);
        Assert.AreEqual(idsA[2], idsB[1]);
    }

    [TestMethod]
    public async Task PlayersReceiveOpponentNames()
    {
        var a = await Connect();
        var b = await Connect();

        await a.Client.SendName("Alice");
        await b.Client.SendName("Bob");
        await a.Client.JoinGame();
        await a.WaitFor(m => m == "WAITING_FOR_OPPONENT");
        await b.Client.JoinGame();

        await a.WaitFor(m => m == "OPPONENT_NAME|Bob");
        await b.WaitFor(m => m == "OPPONENT_NAME|Alice");
    }

    private static Guid IdOf(string message, string name) =>
        Guid.Parse(message.Split('|').Skip(1).First(f => f.EndsWith("," + name)).Split(',')[0]);

    private async Task<(TestClient A, TestClient B, Guid AId, Guid BId)> EnterLobbies()
    {
        var a = await Connect();
        var b = await Connect();
        await a.Client.SendName("Alice");
        await b.Client.SendName("Bob");
        await a.Client.EnterLobby();
        await b.Client.EnterLobby();
        var listForA = await a.WaitFor(m => m.StartsWith("LOBBY_LIST|") && m.Contains(",Bob"));
        var listForB = await b.WaitFor(m => m.StartsWith("LOBBY_LIST|") && m.Contains(",Alice"));
        return (a, b, IdOf(listForB, "Alice"), IdOf(listForA, "Bob"));
    }

    [TestMethod]
    public async Task LobbyListsOtherPlayersAndDropsDisconnected()
    {
        var (a, b, _, _) = await EnterLobbies();
        b.Dispose();
        await a.WaitFor(m => m == "LOBBY_LIST");
    }

    [TestMethod]
    public async Task AcceptedChallengeStartsGame()
    {
        var (a, b, aId, bId) = await EnterLobbies();

        await a.Client.Challenge(bId);
        await b.WaitFor(m => m == $"CHALLENGED|{aId},Alice");
        await b.Client.AcceptChallenge(aId);

        await a.WaitFor(m => m == "OPPONENT_NAME|Bob");
        await a.WaitFor(m => m == "YOU_ARE_PLAYER_1");
        await b.WaitFor(m => m == "YOU_ARE_PLAYER_2");
    }

    [TestMethod]
    public async Task DeclinedChallengeNotifiesChallenger()
    {
        var (a, b, aId, bId) = await EnterLobbies();

        await a.Client.Challenge(bId);
        await b.WaitFor(m => m.StartsWith("CHALLENGED|"));
        await b.Client.DeclineChallenge(aId);

        await a.WaitFor(m => m == $"CHALLENGE_DECLINED|{bId}");
    }

    [TestMethod]
    public async Task ChallengingUnknownPlayerIsRejected()
    {
        var (a, _, _, _) = await EnterLobbies();
        await a.Client.Challenge(Guid.NewGuid());
        await a.WaitFor(m => m == "ERROR PLAYER_UNAVAILABLE");
    }

    [TestMethod]
    public async Task LobbyRequiresName()
    {
        var a = await Connect();
        await a.Client.EnterLobby();
        await a.WaitFor(m => m == "ERROR NAME_REQUIRED");
    }

    [TestMethod]
    public async Task JoiningTwiceIsRejected()
    {
        var a = await Connect();
        await a.Client.JoinGame();
        await a.WaitFor(m => m == "WAITING_FOR_OPPONENT");
        await a.Client.JoinGame();
        await a.WaitFor(m => m == "ERROR ALREADY_WAITING");
    }

    [TestMethod]
    public async Task MoveIsRelayedToOpponent()
    {
        var (a, b) = await StartMatch();

        await a.Client.SendMove("QueenBee", 0, 0, 0, 0, 0, 0, "LIGHT");

        await b.WaitFor(m => m.StartsWith("MOVE|LIGHT|QueenBee|0,0,0|0,0,0"));
    }

    [TestMethod]
    public async Task MovesAlternateBetweenPlayers()
    {
        var (a, b) = await StartMatch();

        await a.Client.SendMove("QueenBee", 0, 0, 0, 0, 0, 0);
        await b.WaitFor(m => m.StartsWith("MOVE|"));
        await b.Client.SendMove("QueenBee", 1, -1, 0, 1, -1, 0, "DARK");

        await a.WaitFor(m => m.StartsWith("MOVE|DARK|QueenBee|1,-1,0|1,-1,0"));
    }

    [TestMethod]
    public async Task OutOfTurnMoveIsRejected()
    {
        var (_, b) = await StartMatch();

        await b.Client.SendMove("QueenBee", 0, 0, 0, 0, 0, 0, "DARK");

        await b.WaitFor(m => m == "MOVE_REJECTED NOT_YOUR_TURN");
    }

    [TestMethod]
    public async Task MoveWithoutGameIsRejected()
    {
        var a = await Connect();

        await a.Client.SendMove("QueenBee", 0, 0, 0, 0, 0, 0);

        await a.WaitFor(m => m == "ERROR NOT_IN_GAME");
    }

    [TestMethod]
    public async Task ChatIsRelayedBothWays()
    {
        var (a, b) = await StartMatch();

        await a.Client.SendChat("hello from light");
        await b.WaitFor(m => m == "CHAT|hello from light");
        await b.Client.SendChat("hi from dark");
        await a.WaitFor(m => m == "CHAT|hi from dark");
    }

    [TestMethod]
    public async Task ChatWithoutGameIsRejected()
    {
        var a = await Connect();

        await a.Client.SendChat("anyone?");

        await a.WaitFor(m => m == "ERROR NOT_IN_GAME");
    }

    [TestMethod]
    public async Task RematchBeforeGameIsOverIsRejected()
    {
        var (a, _) = await StartMatch();

        await a.Client.SendMessage("REMATCH");

        await a.WaitFor(m => m == "ERROR GAME_NOT_FINISHED");
    }

    [TestMethod]
    public async Task RematchWithoutGameIsRejected()
    {
        var a = await Connect();

        await a.Client.SendMessage("REMATCH");

        await a.WaitFor(m => m == "ERROR NOT_IN_GAME");
    }

    [TestMethod]
    public async Task OpponentIsNotifiedOnDisconnect()
    {
        var (a, b) = await StartMatch();

        a.Client.Disconnect();

        await b.WaitFor(m => m == "OPPONENT_DISCONNECTED");
    }

    private async Task<(TestClient a, TestClient b)> StartMatch()
    {
        var a = await Connect();
        var b = await Connect();
        await a.Client.JoinGame();
        await a.WaitFor(m => m == "WAITING_FOR_OPPONENT");
        await b.Client.JoinGame();
        await a.WaitFor(m => m == "YOU_ARE_PLAYER_1");
        await b.WaitFor(m => m == "YOU_ARE_PLAYER_2");
        return (a, b);
    }

    private async Task<TestClient> Connect()
    {
        var c = new TestClient(_port);
        _clients.Add(c);
        await c.WaitFor(m => m.StartsWith("WELCOME "));
        return c;
    }

    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private sealed class TestClient : IDisposable
    {
        private readonly List<string> _messages = new();
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cts = new();
        private int _cursor;

        public HiveGameClient Client { get; }

        public TestClient(int port)
        {
            Client = new HiveGameClient("localhost", port);
            Client.MessageReceived += m => { lock (_lock) _messages.Add(m); };
            _ = Task.Run(() => Client.ConnectAsync(_cts.Token));
        }

        // Waits for a message matching the predicate, consuming everything before it.
        public async Task<string> WaitFor(Func<string, bool> predicate)
        {
            var deadline = DateTime.UtcNow + Timeout;
            while (DateTime.UtcNow < deadline)
            {
                lock (_lock)
                {
                    while (_cursor < _messages.Count)
                    {
                        var m = _messages[_cursor++];
                        if (predicate(m))
                            return m;
                    }
                }
                await Task.Delay(20);
            }

            string seen;
            lock (_lock) seen = string.Join(", ", _messages);
            throw new AssertFailedException($"Timed out waiting for message. Received: [{seen}]");
        }

        public void Dispose()
        {
            _cts.Cancel();
            Client.Dispose();
        }
    }
}
