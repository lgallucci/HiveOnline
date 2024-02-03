using HiveContracts;
using HiveGraphics;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.GameEngines
{
    class OpeningScreenEngine : GameEngine
    {
        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _graphicsEngine.DrawString("HIVE !   Click to play...");
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            
        }

        public override void Update(ref GameState _gameState)
        {
            var mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _gameState = GameState.Playing;
            }
        }
    }
}
