using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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

        // Encrypt the connection with TLS. If ServerKeyPin (SHA-256 hex of the server's public key, see
        // HiveServer --generate-cert) is set the key is pinned, otherwise the certificate must be trusted by the system.
        public bool UseTls { get; init; }
        public string ServerKeyPin { get; init; }

        // A player-friendly description of why the connection is currently down; null while connected or still
        // making the first attempt. The client keeps retrying in the background.
        public string ConnectionProblem => _connectionProblem;
        private volatile string _connectionProblem;

        public string Address => _address;
        public int Port => _port;

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

            _tcpClient.Message += OnTransportMessage;
            _tcpClient.ClosedCallback = (_, remote) =>
            {
                if (remote)
                    _connectionProblem = "Connection to the server was lost.";
            };

            if (UseTls)
            {
                _tcpClient.StreamSetup = async stream =>
                {
                    var ssl = new SslStream(stream, false, ValidateServerCertificate);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = _address }, timeout.Token);
                    return ssl;
                };
            }

            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                _tcpClient.AutoReconnect = false;
                _tcpClient.Disconnect();
            });

            await _tcpClient.RunAsync();
        }

        private void OnTransportMessage(object sender, AsyncTcpEventArgs e)
        {
            switch (e.Message)
            {
                case "Connection timeout":
                    _connectionProblem = $"Connecting to {_address}:{_port} timed out.";
                    break;
                case "Error connecting to remote host":
                    _connectionProblem = $"Could not reach the server at {_address}:{_port}.";
                    break;
                case "Secure connection setup failed":
                    _connectionProblem = "Could not establish a secure connection. The server may be unencrypted, or its identity did not match.";
                    break;
                case "Connection reset remotely":
                case "Connection aborted":
                    _connectionProblem = "Connection to the server was lost.";
                    break;
            }
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (string.IsNullOrWhiteSpace(ServerKeyPin))
                return errors == SslPolicyErrors.None;

            if (certificate == null)
                return false;

            var expected = ServerKeyPin.Replace(":", "").Replace(" ", "");
            var actual = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(new X509Certificate2(certificate).PublicKey.ExportSubjectPublicKeyInfo()));
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
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
            _connectionProblem = null;
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

        public async Task SendMove(string pieceType, int fromQ, int fromR, int fromS, int toQ, int toR, int toS, string team = "LIGHT", string copiedType = null)
        {
            var payload = $"MOVE|{team}|{pieceType}|{fromQ},{fromR},{fromS}|{toQ},{toR},{toS}";
            if (!string.IsNullOrEmpty(copiedType))
                payload += $"|{copiedType}";
            await SendMessage(payload);
        }

        public async Task SendName(string name)
        {
            var clean = name.Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim();
            if (clean.Length > 0)
                await SendMessage($"NAME|{clean}");
        }

        public Task SendExpansions(string codes) => SendMessage($"EXPANSIONS|{codes}");

        public Task EnterLobby() => SendMessage("LOBBY");

        public Task LeaveLobby() => SendMessage("LEAVE_LOBBY");

        public Task Challenge(Guid playerId) => SendMessage($"CHALLENGE|{playerId}");

        public Task AcceptChallenge(Guid playerId) => SendMessage($"ACCEPT|{playerId}");

        public Task DeclineChallenge(Guid playerId) => SendMessage($"DECLINE|{playerId}");

        public async Task SendChat(string text)
        {
            var clean = text.Replace("\r", " ").Replace("\n", " ").Trim();
            if (clean.Length > 0)
                await SendMessage($"CHAT|{clean}");
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