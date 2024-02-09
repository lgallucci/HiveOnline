using HiveContracts;
using HiveGraphics.GameAssetsDraw;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace HiveOnline.GameAssets
{
    public class Pile
    {
        public Stack<Beetle> Beetles { get; set; }
        public Stack<Grasshopper> Grasshoppers { get; set; }
        public Stack<LadyBug> LadyBugs { get; set; }
        public Stack<Mosquito> Mosquitos { get; set; }
        public Stack<PillBug> PillBugs { get; set; }
        public Stack<QueenBee> QueenBees { get; set; }
        public Stack<SoldierAnt> SoldierAnts { get; set; }
        public Stack<Spider> Spiders { get; set; }
        private BugTeam _team { get; set; }

        private int _tileSize = 75;
        public HexPoint BeetleLocation { get; set; }
        public HexPoint GrasshoppersLocation { get; set; }
        public HexPoint LadyBugsLocation { get; set; }
        public HexPoint MosquitosLocation { get; set; }
        public HexPoint PillBugsLocation { get; set; }
        public HexPoint QueenBeesLocation { get; set; }
        public HexPoint SoldierAntsLocation { get; set; }
        public HexPoint SpidersLocation { get; set; }

        private PileGraphics Graphics { get; set; }

        public Pile(BugTeam team,
                    int beetleCount = 2,
                    int grasshopperCount = 3,
                    int ladyBugCount = 0,
                    int mosquitoCount = 0,
                    int pillBugCount = 0,
                    int queenBeeCount = 1,
                    int soldierAntCount = 3,
                    int spiderCount = 2)
        {
            Graphics = new PileGraphics(_tileSize);
            _team = team;
            while (beetleCount > 0 || grasshopperCount > 3 || ladyBugCount > 0 || mosquitoCount > 0 || pillBugCount > 0 || queenBeeCount > 0 || soldierAntCount > 0 || spiderCount > 0)
            {
                if (beetleCount > 0)
                {
                    if (Beetles == null)
                    {
                        Beetles = new Stack<Beetle>();
                        _stackCount++;
                    }
                    Beetles.Push(new Beetle(_team) { Location = new Hex(-100, 50, 50) });
                    beetleCount--;
                }
                if (grasshopperCount > 0)
                {
                    if (Grasshoppers == null)
                    {
                        Grasshoppers = new Stack<Grasshopper>();
                        _stackCount++;
                    }
                    Grasshoppers.Push(new Grasshopper(_team) { Location = new Hex(-100, 50, 50) });
                    grasshopperCount--;
                }
                if (ladyBugCount > 0)
                {
                    if (LadyBugs == null)
                    {
                        LadyBugs = new Stack<LadyBug>();
                        _stackCount++;
                    }
                    LadyBugs.Push(new LadyBug(_team) { Location = new Hex(-100, 50, 50) });
                    ladyBugCount--;
                }
                if (mosquitoCount > 0)
                {
                    if (Mosquitos == null)
                    {
                        Mosquitos = new Stack<Mosquito>();
                        _stackCount++;
                    }
                    Mosquitos.Push(new Mosquito(_team) { Location = new Hex(-100, 50, 50) });
                    mosquitoCount--;
                }
                if (pillBugCount > 0)
                {
                    if (PillBugs == null)
                    {
                        PillBugs = new Stack<PillBug>();
                        _stackCount++;
                    }
                    PillBugs.Push(new PillBug(_team) { Location = new Hex(-100, 50, 50) });
                    pillBugCount--;
                }
                if (queenBeeCount > 0)
                {
                    if (QueenBees == null)
                    {
                        QueenBees = new Stack<QueenBee>();
                        _stackCount++;
                    }
                    QueenBees.Push(new QueenBee(_team) { Location = new Hex(-100, 50, 50) });
                    queenBeeCount--;
                }
                if (soldierAntCount > 0)
                {
                    if (SoldierAnts == null)
                    {
                        SoldierAnts = new Stack<SoldierAnt>();
                        _stackCount++;
                    }
                    SoldierAnts.Push(new SoldierAnt(_team) { Location = new Hex(-100, 50, 50) });
                    soldierAntCount--;
                }
                if (spiderCount > 0)
                {
                    if (Spiders == null)
                    {
                        Spiders = new Stack<Spider>();
                        _stackCount++;
                    }
                    Spiders.Push(new Spider(_team) { Location = new Hex(-100, 50, 50) });
                    spiderCount--;
                }
            }
        }

        private int _stackCount = 0;
        private int _placementWidth = 100;
        internal void ChangeScreenSize(int width, int height, bool isOpponent)
        {
            Graphics.ChangeScreenSize(width, height, _stackCount, _placementWidth, isOpponent);
            ResizePile();
        }

        public bool Intersects(int x, int y)
        {
            if (Graphics.Location.Contains(x, y)) return true;
            return false;
        }

        public ITile GetIntersectBug(int x, int y)
        {
            if (BeetleLocation.Contains(x, y, _tileSize, _tileSize) && Beetles.Count > 0)
                return Beetles.Peek();

            if (GrasshoppersLocation.Contains(x, y, _tileSize, _tileSize) && Grasshoppers.Count > 0)
                return Grasshoppers.Peek();

            if (LadyBugsLocation.Contains(x, y, _tileSize, _tileSize) && LadyBugs.Count > 0)
                return LadyBugs.Peek();

            if (MosquitosLocation.Contains(x, y, _tileSize, _tileSize) && Mosquitos.Count > 0)
                return Mosquitos.Peek();

            if (PillBugsLocation.Contains(x, y, _tileSize, _tileSize) && PillBugs.Count > 0)
                return PillBugs.Peek();

            if (QueenBeesLocation.Contains(x, y, _tileSize, _tileSize) && QueenBees.Count > 0)
                return QueenBees.Peek();

            if (SoldierAntsLocation.Contains(x, y, _tileSize, _tileSize) && SoldierAnts.Count > 0)
                return SoldierAnts.Peek();

            if (SpidersLocation.Contains(x, y, _tileSize, _tileSize) && Spiders.Count > 0)
                return Spiders.Peek();

            return null;
        }

        public void Draw()
        {
            Graphics.DrawBox();

            if (Beetles?.Count > 0)
            {
                Graphics.DrawBug((location, size) => Beetles.Peek().Draw(location, size), BeetleLocation,
                $"x{Beetles.Count}");
            }
            if (Grasshoppers?.Count > 0)
            {
                Graphics.DrawBug((location, size) => Grasshoppers.Peek().Draw(location, size), GrasshoppersLocation,
                    $"x{Grasshoppers.Count}");
            }
            if (LadyBugs?.Count > 0)
            {
                Graphics.DrawBug((location, size) => LadyBugs.Peek().Draw(location, size), LadyBugsLocation,
                    $"x{LadyBugs.Count}");
            }
            if (Mosquitos?.Count > 0)
            {
                Graphics.DrawBug((location, size) => Mosquitos.Peek().Draw(location, size), MosquitosLocation,
                    $"x{Mosquitos.Count}");
            }
            if (PillBugs?.Count > 0)
            {
                Graphics.DrawBug((location, size) => PillBugs.Peek().Draw(location, size), PillBugsLocation,
                    $"x{PillBugs.Count}");
            }
            if (QueenBees?.Count > 0)
            {
                Graphics.DrawBug((location, size) => QueenBees.Peek().Draw(location, size), QueenBeesLocation,
                    $"x{QueenBees.Count}");
            }
            if (SoldierAnts?.Count > 0)
            {
                Graphics.DrawBug((location, size) => SoldierAnts.Peek().Draw(location, size), SoldierAntsLocation,
                    $"x{SoldierAnts.Count}");
            }
            if (Spiders?.Count > 0)
            {
                Graphics.DrawBug((location, size) => Spiders.Peek().Draw(location, size), SpidersLocation,
                    $"x{Spiders.Count}");
            }
        }

        private void ResizePile()
        {
            int bufferSize = 0;
            if (Beetles?.Count > 0)
            {
                BeetleLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (Grasshoppers?.Count > 0)
            {
                GrasshoppersLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (LadyBugs?.Count > 0)
            {
                LadyBugsLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (Mosquitos?.Count > 0)
            {
                MosquitosLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (PillBugs?.Count > 0)
            {
                PillBugsLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (QueenBees?.Count > 0)
            {
                QueenBeesLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (SoldierAnts?.Count > 0)
            {
                SoldierAntsLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
                bufferSize += _placementWidth;
            }
            if (Spiders?.Count > 0)
            {
                SpidersLocation = new HexPoint(Graphics.Location.Left + bufferSize, Graphics.Location.Top);
            }
        }

        public ITile GetTile(BugType bugType)
        {
            ITile tile = null;
            switch (bugType)
            {
                case BugType.Beetle:
                    tile =  Beetles.Pop();
                    break;
                case BugType.Grasshopper:
                    tile =  Grasshoppers.Pop();
                    break;
                case BugType.LadyBug:
                    tile =  LadyBugs.Pop();
                    break;
                case BugType.Mosquito:
                    tile =  Mosquitos.Pop();
                    break;
                case BugType.PillBug:
                    tile =  PillBugs.Pop();
                    break;
                case BugType.QueenBee:
                    tile =  QueenBees.Pop();
                    break;
                case BugType.SoldierAnt:
                    tile =  SoldierAnts.Pop();
                    break;
                case BugType.Spider:
                    tile =  Spiders.Pop();
                    break;
            }
            ResizePile();
            return tile;
        }

        public List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var availableTiles = new List<Hex>();

            if (board.Tiles.Count > 0)
            {
                foreach (var tile in board.Tiles.Where(t => t.Value.Team == BugTeam.Light))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (!board.Tiles.ContainsKey(tile.Value.Location.Neighbor(i).GetHashCode()))
                        {
                            bool _foundDark = false;
                            for (int j = 0; j < 6; j++)
                            {
                                if (board.Tiles.ContainsKey(tile.Value.Location.Neighbor(i).Neighbor(j).GetHashCode()) &&
                                    board.Tiles[tile.Value.Location.Neighbor(i).Neighbor(j).GetHashCode()].Team == BugTeam.Dark)
                                {
                                    _foundDark = true;
                                    break;
                                }
                            }
                            if (!_foundDark && !availableTiles.Contains(tile.Value.Location.Neighbor(i)))
                            {
                                availableTiles.Add(tile.Value.Location.Neighbor(i));
                            }
                        }
                    }
                }
            }
            else
            {
                availableTiles.Add(new Hex(0, 0, 0));
            }
            return availableTiles;
        }

        internal void DrawSelected(PlayingBoard board, BugType bugType)
        {
            HexPoint location;
            switch (bugType)
            {
                case BugType.Beetle:
                    location = BeetleLocation;
                    break;
                case BugType.Grasshopper:
                    location = GrasshoppersLocation;
                    break;
                case BugType.LadyBug:
                    location = LadyBugsLocation;
                    break;
                case BugType.Mosquito:
                    location = MosquitosLocation;
                    break;
                case BugType.PillBug:
                    location = PillBugsLocation;
                    break;
                case BugType.QueenBee:
                    location = QueenBeesLocation;
                    break;
                case BugType.SoldierAnt:
                    location = SoldierAntsLocation;
                    break;
                case BugType.Spider:
                    location = SpidersLocation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bugType));
            }

            board.Graphics.DrawHexagon(GetCornersFromPoint(location), 4, 217, 255);
        }

        private List<HexPoint> GetCornersFromPoint(HexPoint location)
        {
            var halfTileSize = _tileSize / 2;
            var corners = new List<HexPoint>();
            for (int i = 0; i < 6; i++)
            {
                double angle = 2.0 * Math.PI * (0 - i) / 6.0;
                var offset =  new HexPoint(halfTileSize * Math.Cos(angle), halfTileSize * Math.Sin(angle));

                corners.Add(new HexPoint(location.X + offset.X + halfTileSize, location.Y + offset.Y + halfTileSize));
            }
            return corners;
        }
    }
}