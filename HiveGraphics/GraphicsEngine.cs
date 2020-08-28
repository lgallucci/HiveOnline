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
        public static string UserName { get; set; }

        public GraphicsEngine(ContentManager content, GraphicsDevice graphics)
        {
            Art.Load(content, graphics);
        }

        public bool Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond, IBoard board)
        {
            DrawBoard(graphics, spriteBatch, board);
            DrawPiles(graphics, spriteBatch);
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
            foreach (var hex in board.Tiles)
            {
                HiveContracts.Point firstPoint = new HiveContracts.Point(0,0);
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

            //Draw Pieces
            foreach (var tile in board.Tiles)
            {
                var center = tile.Location;
            }
        }

        private void DrawPiles(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            //Draw Your Pile
            //Draw Opponents Pile
        }
    }
}
