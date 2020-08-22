using HiveClient;
using HiveGraphics;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline
{
    class Game
    { 
        private GraphicsEngine _graphicsEngine;
        private GameEngine _gameEngine;
        private IBoard _board;
        private bool _running = false;

        public Game()
        {

        }

        internal void Run()
        {
            while (_running)
            {
                var tp1 = DateTime.Now;
                var tp2 = DateTime.Now;
                var frameTimer = new TimeSpan(0);
                var frameCount = 0;
                var framesPerSecond = 0;

                //do Engine initialization
                _graphicsEngine = new GraphicsEngine();
                _gameEngine = new GameEngine();
               

                while (_running)
                {
                    // Handle Timing
                    tp2 = DateTime.Now;
                    TimeSpan elapsedTime = tp2 - tp1;
                    tp1 = tp2;
                    double elapsedTicks = elapsedTime.Ticks;

                    // Handle Frame Update
                    if (!_gameEngine.Run(_board))
                        _running = false;

                    if (!_graphicsEngine.Draw(framesPerSecond, _board))
                        _running = false;

                    frameTimer += elapsedTime;
                    frameCount++;
                    if (frameTimer > TimeSpan.FromSeconds(1))
                    {
                        framesPerSecond = frameCount;
                        frameCount = 0;
                    }
                }

                //CLEAN UP RESOURCES AND EXIT
            }
        }
    }
}
