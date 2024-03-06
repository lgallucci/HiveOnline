using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unclassified.Net;

namespace HiveClient
{
    public class HiveGameClient : IDisposable
    {
        private string _address;
        private int _port;
        private AsyncTcpClient _tcpClient;
        private bool disposedValue;

        public bool IsConnected { get { return _tcpClient?.IsConnected ?? false; } }

        public HiveGameClient(string address, int port)
        {
            _address = address;
            _port = port;
        }

        public async Task Connect()
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
            string message = Encoding.UTF8.GetString(buffer, 0, length);
            Console.WriteLine($"Client Received: {message}");

            return Task.CompletedTask;
        }

        private Task ClientConnected(AsyncTcpClient client, bool isReconnect)
        {
            Console.WriteLine($"Connected! {(isReconnect ? "isReconnect" : "")}");

            return Task.CompletedTask;
        }

        public async Task SendMessage(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _tcpClient.Send(bytes);
        }

        //Reconnect on d/c

        //Get List of Open Games

        //Create Game with server

        //Join Existing Game

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tcpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GameClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}