using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;
using System.Text;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public HiveContracts.Point ScreenSize { get; set; }

        public GraphicsEngine(ContentManager content, GraphicsDevice graphics)
        {
            Art.Load(content, graphics);
        }

        public bool Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, int framesPerSecond, Board board)
        {
            DrawBoard(graphics, spriteBatch, board, bloomFilter);
            DrawPiles(graphics, spriteBatch, board, bloomFilter);
            board.ChatWindow.Draw(graphics, spriteBatch);
            DrawText(graphics, spriteBatch, board);
            DrawFps(graphics, spriteBatch, framesPerSecond);
            return false;
        }

        private void DrawFps(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond)
        {
            spriteBatch.DrawString(Art.ChatFont, $"FPS: {framesPerSecond}", new Vector2(1, 1), Color.Red);
        }

        private void DrawText(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Board board)
        {
            var userNameSize = Art.NameFont.MeasureString(board.UserName);
            var opponentNameSize = Art.NameFont.MeasureString(board.OpponentName ?? "TestOpponent");
            spriteBatch.DrawString(Art.NameFont, board.UserName ?? "TestUser", new Vector2(5, /*window height - font height*/(float)ScreenSize.Y - userNameSize.Y - 75), Color.CornflowerBlue);
            spriteBatch.DrawString(Art.NameFont, board.OpponentName ?? "TestOpponent", new Vector2(/*window width - font width*/(float)ScreenSize.X - opponentNameSize.X - 5, 75), Color.Red);
        }

        public static bool SetupUser()
        {
            throw new NotImplementedException();
        }

        private void DrawBoard(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Board board, BloomFilter bloomFilter)
        {
            //Draw Grid
            foreach (var tile in board.Tiles)
            {
                tile.Draw(graphics, spriteBatch, bloomFilter, board);
            }
        }

        private void DrawPiles(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Board board, BloomFilter bloomFilter)
        {
            board.UserPile.Draw(graphics, spriteBatch, bloomFilter);
            board.OpponentPile.Draw(graphics, spriteBatch, bloomFilter);
        }
    }
}
