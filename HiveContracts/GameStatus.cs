using System;

namespace HiveContracts
{

    public struct GameStatus
    {
        public GameState State { get; set; }
        public bool MyTurn { get; set; }
        public ITile SelectedTile { get; set; }
    }
}
