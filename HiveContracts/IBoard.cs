using HiveContracts;
using System.Collections;
using System.Collections.Generic;

namespace HiveContracts
{
    public interface IBoard
    {
        List<ITile> Tiles { get; set; }
        Layout Layout { get; set; }
        Dictionary<int, ITile> HexCoordinates { get; set; }
        Point WindowSize { get; set; }
        string UserName { get; set; }
        string OpponentName { get; set; }
        string TypingText { get; set; }
        Stack<string> ChatMessages { get; set; }

        void AddTile(ITile tile);
        void RemoveTile(ITile tile);
        bool CanMove(ITile piece);
    }
}