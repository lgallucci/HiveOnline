using HiveContracts;
using HiveGraphics.GameAssetsDraw;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                    Beetles.Push(new Beetle(_team));
                    beetleCount--;
                }
                if (grasshopperCount > 0)
                {
                    if (Grasshoppers == null)
                    {
                        Grasshoppers = new Stack<Grasshopper>();
                        _stackCount++;
                    }
                    Grasshoppers.Push(new Grasshopper(_team));
                    grasshopperCount--;
                }
                if (ladyBugCount > 0)
                {
                    if (LadyBugs == null)
                    {
                        LadyBugs = new Stack<LadyBug>();
                        _stackCount++;
                    }
                    LadyBugs.Push(new LadyBug(_team));
                    ladyBugCount--;
                }
                if (mosquitoCount > 0)
                {
                    if (Mosquitos == null)
                    {
                        Mosquitos = new Stack<Mosquito>();
                        _stackCount++;
                    }
                    Mosquitos.Push(new Mosquito(_team));
                    mosquitoCount--;
                }
                if (pillBugCount > 0)
                {
                    if (PillBugs == null)
                    {
                        PillBugs = new Stack<PillBug>();
                        _stackCount++;
                    }
                    PillBugs.Push(new PillBug(_team));
                    pillBugCount--;
                }
                if (queenBeeCount > 0)
                {
                    if (QueenBees == null)
                    {
                        QueenBees = new Stack<QueenBee>();
                        _stackCount++;
                    }
                    QueenBees.Push(new QueenBee(_team));
                    queenBeeCount--;
                }
                if (soldierAntCount > 0)
                {
                    if (SoldierAnts == null)
                    {
                        SoldierAnts = new Stack<SoldierAnt>();
                        _stackCount++;
                    }
                    SoldierAnts.Push(new SoldierAnt(_team));
                    soldierAntCount--;
                }
                if (spiderCount > 0)
                {
                    if (Spiders == null)
                    {
                        Spiders = new Stack<Spider>();
                        _stackCount++;
                    }
                    Spiders.Push(new Spider(_team));
                    spiderCount--;
                }
            }
        }

        private int _stackCount = 0;
        private int _placementWidth = 100;
        internal void ChangeScreenSize(int width, int height, bool isOpponent)
        {
            Graphics.ChangeScreenSize(width, height, _stackCount, _placementWidth, isOpponent);
        }

        public void Draw(PlayingBoard board)
        {
            Graphics.DrawBox();
            var halfTileSize = _tileSize / 2;

            int bufferSize = 0;
            if (Beetles?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => Beetles.Peek().Draw(location, size),
                    $"x{Beetles.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (Grasshoppers?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => Grasshoppers.Peek().Draw(location, size),
                    $"x{Grasshoppers.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (LadyBugs?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => LadyBugs.Peek().Draw(location, size),
                    $"x{LadyBugs.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (Mosquitos?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => Mosquitos.Peek().Draw(location, size),
                    $"x{Mosquitos.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (PillBugs?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => PillBugs.Peek().Draw(location, size),
                    $"x{PillBugs.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (QueenBees?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => QueenBees.Peek().Draw(location, size),
                    $"x{QueenBees.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (SoldierAnts?.Count > 0)
            {
                var location = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => SoldierAnts.Peek().Draw(location, size),
                    $"x{SoldierAnts.Count}", bufferSize);

                bufferSize += _placementWidth;
            }
            if (Spiders?.Count > 0)
            {
                var bugLocation = new HexPoint(Graphics.Location.Left + bufferSize + halfTileSize, Graphics.Location.Top + halfTileSize);
                Graphics.DrawBug((location, size) => Spiders.Peek().Draw(location, size),
                    $"x{Spiders.Count}", bufferSize);
            }
        }

        public ITile GetTile(BugType bugType)
        {
            switch (bugType)
            {
                case BugType.Beetle:
                    return Beetles.Pop();
                case BugType.Grasshopper:
                    return Grasshoppers.Pop();
                case BugType.LadyBug:
                    return LadyBugs.Pop();
                case BugType.Mosquito:
                    return Mosquitos.Pop();
                case BugType.PillBug:
                    return PillBugs.Pop();
                case BugType.SoldierAnt:
                    return SoldierAnts.Pop();
                case BugType.Spider:
                    return Spiders.Pop();
            }
            return null;
        }
    }
}