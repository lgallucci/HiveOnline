using HiveClient;
using HiveOnline.GameAssets;
using System.Drawing;

namespace HiveOnline
{
    class GameEngine
    {
        private HiveGameClient _hiveClient;
        private string _userName = string.Empty;
        private string _address = string.Empty;
        private string _key = string.Empty;

        internal bool Run(ref IBoard board)
        {
            //CONNNECT TO SERVER
            _hiveClient = new HiveGameClient(_address, _key, _userName);

            //if (!_hiveClient.Connect())
            //{
            //    return false;
            //}

            
            //Handle Inputs


            //Update game state

            return true;
        }
    }

    struct GameInputs
    {
        //public bool MouseClicked;
        //public Point MousePosition;
    }
}
