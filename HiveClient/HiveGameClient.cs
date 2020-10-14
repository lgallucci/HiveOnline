using System;
using System.Net.Sockets;
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

        private Task ClientReceived(AsyncTcpClient arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        private Task ClientConnected(AsyncTcpClient arg1, bool arg2)
        {
            throw new NotImplementedException();
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
                    // TODO: dispose managed state (managed objects)
                    _tcpClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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