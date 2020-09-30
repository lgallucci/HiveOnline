using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public string UserName { get; set; }

        public GraphicsEngine(ContentManager content, GraphicsDevice graphics)
        {
            Art.Load(content, graphics);
        }

        public bool Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond, IBoard board)
        {
            DrawBoard(graphics, spriteBatch, board);
            DrawPiles(graphics, spriteBatch, board);
            DrawChat(graphics, spriteBatch);
            DrawText(graphics, spriteBatch);
            DrawFps(graphics, spriteBatch, framesPerSecond);
            return false;
        }

        private void DrawFps(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond)
        {
            spriteBatch.DrawString(Art.ArialFont, $"FPS: {framesPerSecond}", new Vector2(1, 1), Color.Red);
        }

        private void DrawText(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Art.NameFont, $"Name1", new Vector2(1, /*window height - font height*/735), Color.Red);
            spriteBatch.DrawString(Art.NameFont, $"Name2", new Vector2(/*window width - font width*/920, 1), Color.Red);
        }

        private void DrawChat(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {

        }

        public static bool SetupUser()
        {
            throw new NotImplementedException();
        }

        private void DrawBoard(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            //Draw Grid
            foreach (var tile in board.Tiles)
            {
                switch(tile.Type)
                {
                    case BugType.Blank:
                        DrawBlankTile(spriteBatch, board, tile);
                        break;
                    default:
                        DrawBugTile(spriteBatch, board, tile);
                        break;
                }
            }

            //Draw Pieces
            foreach (var tile in board.Tiles)
            {
                var center = tile.Location;
            }
        }

        private void DrawBugTile(SpriteBatch spriteBatch, IBoard board, ITile tile)
        {
            var tileSize = board.Layout.size;
            var texture = GetTextureFromTile(tile);
            spriteBatch.Draw(
                texture, 
                board.Layout.HexToPixel(tile.Location).ToVector2(),
                new Rectangle(0, 0, texture.Width, texture.Height), 
                Color.White,
                0,
                new Vector2(texture.Width / 2, texture.Height / 2), 
                new Vector2((float)tileSize.x / (texture.Width - 100), (float)tileSize.y / (texture.Height - 100)), //Scale
                SpriteEffects.None, 0f);
        }

        private Texture2D GetTextureFromTile(ITile tile)
        {
            switch(tile.Team)
            {
                case BugTeam.Dark:
                    switch(tile.Type) 
                    {
                        case BugType.Beetle:
                            return Art.DarkBeetle;
                        case BugType.Grasshopper:
                            return Art.DarkGrassHopper;
                        case BugType.LadyBug: 
                            return Art.DarkLadyBug;
                        case BugType.Mosquito:
                            return Art.DarkMosquito;
                        case BugType.PillBug:
                            return Art.DarkPillBug;
                        case BugType.QueenBee:
                            return Art.DarkQueenBee;
                        case BugType.SoldierAnt:
                            return Art.DarkSoldierAnt;
                        case BugType.Spider:
                            return Art.DarkSpider;
                        default:
                            break;
                    }
                    break;
                case BugTeam.Light:
                    switch (tile.Type)
                    {
                        case BugType.Beetle:
                            return Art.LightBeetle;
                        case BugType.Grasshopper:
                            return Art.LightGrassHopper;
                        case BugType.LadyBug:
                            return Art.LightLadyBug;
                        case BugType.Mosquito:
                            return Art.LightMosquito;
                        case BugType.PillBug:
                            return Art.LightPillBug;
                        case BugType.QueenBee:
                            return Art.LightQueenBee;
                        case BugType.SoldierAnt:
                            return Art.LightSoldierAnt;
                        case BugType.Spider:
                            return Art.LightSpider;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return Art.Pixel;
        }

        private void DrawBlankTile(SpriteBatch spriteBatch, IBoard board, ITile hex)
        {
            HiveContracts.Point firstPoint = new HiveContracts.Point(0, 0);
            Nullable<HiveContracts.Point> previousPoint = null;
            foreach (var corner in board.Layout.PolygonCorners(hex.Location))
            {
                if (previousPoint.HasValue)
                {
                    spriteBatch.DrawLine(corner.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
                }
                else
                {
                    firstPoint = corner;
                }

                previousPoint = corner;
            }

            spriteBatch.DrawLine(firstPoint.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
            var vector2 = board.Layout.HexToPixel(hex.Location);
            spriteBatch.DrawString(Art.ArialFont, $"{hex.Location.q}, {hex.Location.r}, {hex.Location.s}",
                new Vector2((float)vector2.x - 20, (float)vector2.y - 7), Color.Red);
        }

        private void DrawPiles(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            //Draw Your Pile            
            //Draw Opponents Pile
        }
    }
}
