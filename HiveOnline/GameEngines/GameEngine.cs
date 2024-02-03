using HiveContracts;
using HiveOnline.GameAssets;
using System;

namespace HiveOnline
{

    public abstract class GameEngine
    {
        public abstract void Update(ref GameState _gameState);

        public abstract void SetScreenSize(int screenWidth, int screenHeight);

        public abstract void Draw(HiveGraphics.GraphicsEngine _graphicsEngine);
    }
}
