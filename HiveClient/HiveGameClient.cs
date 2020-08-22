using System;

namespace HiveClient
{
    public class HiveGameClient
    {
        private string address;
        private string key;
        private string userName;

        public HiveGameClient(string address, string key, string userName)
        {
            this.address = address;
            this.key = key;
            this.userName = userName;
        }
         
        public bool Connect()
        {
            throw new NotImplementedException();
        }

        //Reconnect on d/c

        //Get List of Open Games

        //Create Game with server

        //Join Existing Game
    }
}
