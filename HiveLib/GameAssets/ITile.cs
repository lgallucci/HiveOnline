using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.GameAssets
{
    public interface ITile : IEquitable<Hex>
    {
        BugType Type { get; set; }
        BugTeam Team { get; set; }
        Hex Location { get; set; }
        bool IsInspecting { get; set; }

        List<Hex> CalculateAvailable(PlayingBoard board);
        bool CanMove(PlayingBoard board);
        bool CanMoveTo(PlayingBoard board, Hex position);
        void Draw(PlayingBoard board);
        void Draw(HexPoint location, HexPoint tileSize);
        void RunAddRules(ITile tile);
        ITile RunRemoveRules();
        bool Equals(Hex other);
    }
}
