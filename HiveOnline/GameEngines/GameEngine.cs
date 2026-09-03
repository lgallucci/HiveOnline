using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using System;

namespace HiveOnline
{

    public abstract class GameEngine
    {
        public abstract void Update(GameTime gameTime, ref GameState _gameState);

        public abstract void SetScreenSize(int screenWidth, int screenHeight);

        public abstract void Draw(HiveGraphics.GraphicsEngine _graphicsEngine);
    }
}
