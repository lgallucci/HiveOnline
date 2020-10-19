using HiveContracts;
using HiveLib;
using HiveLib.Bugs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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
        private Rectangle _location { get; set; }

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
        private int _tileSize = 75;
        private int _placementWidth = 100;
        internal void ChangeScreenSize(int width, int height, bool isOpponent)
        {
            var pileWidth = _stackCount * _placementWidth;
            if (!isOpponent)
                _location = new Rectangle(5, height - 75, pileWidth, 75);
            else
                _location = new Rectangle(width - pileWidth - 5, 5, pileWidth, 75);
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter)
        {
            int halfTileSize = _tileSize / 2;

            //DRAW BOX
            spriteBatch.Draw(Art.Pixel, _location, new Color(48, 90, 70));

            int bufferSize = 0;
            if (Beetles?.Count > 0)
            {
                var numberString = $"x{Beetles.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new Beetle(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (Grasshoppers?.Count > 0)
            {
                var numberString = $"x{Grasshoppers.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new Grasshopper(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (LadyBugs?.Count > 0)
            {
                var numberString = $"x{LadyBugs.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new LadyBug(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (Mosquitos?.Count > 0)
            {
                var numberString = $"x{Mosquitos.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new Mosquito(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (PillBugs?.Count > 0)
            {
                var numberString = $"x{PillBugs.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new PillBug(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (QueenBees?.Count > 0)
            {
                var numberString = $"x{QueenBees.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new QueenBee(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (SoldierAnts?.Count > 0)
            {
                var numberString = $"x{SoldierAnts.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new SoldierAnt(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
                bufferSize += _placementWidth;
            }
            if (Spiders?.Count > 0)
            {
                var numberString = $"x{Spiders.Count}";
                var fontSize = Art.PileFont.MeasureString(numberString);
                new Spider(_team).Draw(graphics, spriteBatch, bloomFilter, 
                    new Vector2(_location.Left + bufferSize + halfTileSize, _location.Top + halfTileSize), new HiveContracts.Point(_tileSize, _tileSize));
                spriteBatch.DrawString(Art.PileFont, numberString, new Vector2(_location.Left + bufferSize + _tileSize - 5, _location.Bottom - fontSize.Y), Color.MintCream);
            }
        }
    }
}