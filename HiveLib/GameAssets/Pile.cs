using HiveContracts;
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
            return tile;
        }

        public int GetCount(BugType bugType)
        {
            return bugType switch
            {
                BugType.Beetle => Beetles?.Count ?? 0,
                BugType.Grasshopper => Grasshoppers?.Count ?? 0,
                BugType.LadyBug => LadyBugs?.Count ?? 0,
                BugType.Mosquito => Mosquitos?.Count ?? 0,
                BugType.PillBug => PillBugs?.Count ?? 0,
                BugType.QueenBee => QueenBees?.Count ?? 0,
                BugType.SoldierAnt => SoldierAnts?.Count ?? 0,
                BugType.Spider => Spiders?.Count ?? 0,
                _ => 0
            };
        }

        public ITile Peek(BugType bugType)
        {
            return bugType switch
            {
                BugType.Beetle => Beetles?.Count > 0 ? Beetles.Peek() : null,
                BugType.Grasshopper => Grasshoppers?.Count > 0 ? Grasshoppers.Peek() : null,
                BugType.LadyBug => LadyBugs?.Count > 0 ? LadyBugs.Peek() : null,
                BugType.Mosquito => Mosquitos?.Count > 0 ? Mosquitos.Peek() : null,
                BugType.PillBug => PillBugs?.Count > 0 ? PillBugs.Peek() : null,
                BugType.QueenBee => QueenBees?.Count > 0 ? QueenBees.Peek() : null,
                BugType.SoldierAnt => SoldierAnts?.Count > 0 ? SoldierAnts.Peek() : null,
                BugType.Spider => Spiders?.Count > 0 ? Spiders.Peek() : null,
                _ => null
            };
        }

        public List<Hex> CalculateAvailable(PlayingBoard board, BugTeam team = BugTeam.Light)
        {
            var availableTiles = new List<Hex>();
            var availableKeys = new HashSet<int>();

            if (board.Tiles.Count == 0)
            {
                availableTiles.Add(new Hex(0, 0, 0));
                return availableTiles;
            }

            var hasTeamTile = board.Tiles.Values.Any(t => t.Team == team);

            foreach (var tile in board.Tiles.Values)
            {
                if (hasTeamTile && tile.Team != team)
                    continue;

                for (int i = 0; i < 6; i++)
                {
                    var candidate = tile.Location.Neighbor(i);
                    if (board.Tiles.ContainsKey(candidate.GetHashCode()) || !availableKeys.Add(candidate.GetHashCode()))
                        continue;

                    if (!hasTeamTile)
                    {
                        availableTiles.Add(candidate);
                        continue;
                    }

                    bool foundOpponentNeighbor = false;
                    for (int j = 0; j < 6; j++)
                    {
                        var neighbor = candidate.Neighbor(j);
                        if (board.Tiles.ContainsKey(neighbor.GetHashCode()))
                        {
                            var neighborTile = board.Tiles[neighbor.GetHashCode()];
                            if (neighborTile.Team != team)
                            {
                                foundOpponentNeighbor = true;
                                break;
                            }
                        }
                    }

                    if (!foundOpponentNeighbor)
                    {
                        availableTiles.Add(candidate);
                    }
                }
            }

            return availableTiles;
        }

    }
}