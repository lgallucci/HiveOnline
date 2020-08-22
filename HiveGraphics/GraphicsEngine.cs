using HiveContracts;
using HiveOnline.GameAssets;
using System;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public static string UserName { get; set; }

        public bool Draw(int framesPerSecond, IBoard board, GameStatus gameStatus)
        {
            DrawBoard(board);
            DrawPiles();
            DrawChat();
            DrawText();
            DrawFps(framesPerSecond);
            return false;
        }

        private void DrawFps(int framesPerSecond)
        {
            throw new NotImplementedException();
        }

        private void DrawText()
        {
            throw new NotImplementedException();
        }

        private void DrawChat()
        {
            throw new NotImplementedException();
        }

        public static bool SetupUser()
        {
            throw new NotImplementedException();
        }

        private void DrawBoard(IBoard board)
        {
            //Draw Grid
            //Draw Pieces
            throw new NotImplementedException();
        }

        private void DrawPiles()
        {
            //Draw Your Pile
            //Draw Opponents Pile
            throw new NotImplementedException();
        }
    }
}
