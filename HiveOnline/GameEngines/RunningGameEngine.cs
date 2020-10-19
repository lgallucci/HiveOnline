using HiveClient;
using HiveContracts;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Input;
using System;

namespace HiveOnline
{
    class RunningGameEngine : GameEngine
    {
        public override bool Update(ref Board board)
        {
            Point originPoint = board.Layout.origin;
            Point originalSize = board.Layout.size;

            var mouseState = Mouse.GetState();
            var fractionalHex = board.Layout.PixelToHex(new Point(mouseState.X, mouseState.Y));
            var hexHash = fractionalHex.HexRound().GetHashCode();
            if (board.HexCoordinates.ContainsKey(hexHash) && !draggingCamera)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    var tile = board.HexCoordinates[hexHash];
                    for (int i = 0; i < 6; i++)
                    {
                        var newHex = tile.Location.Neighbor(i);
                        if (!board.HexCoordinates.ContainsKey(newHex.GetHashCode()))
                        {
                            board.AddTile(NewRandomTile(newHex));
                        }
                    }
                }
                else if (mouseState.RightButton == ButtonState.Pressed)
                {
                    var tile = board.HexCoordinates[hexHash];
                    if (board.CanMove(tile))
                    {
                        board.RemoveTile(tile);
                    }
                }
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {

                    Mouse.SetCursor(MouseCursor.Crosshair);
                    draggingCamera = true;
                    if (lastDragPosition == default(Point))
                        lastDragPosition = new Point(mouseState.X, mouseState.Y);

                    originPoint = HandleCameraDrag(board, mouseState);

                    lastDragPosition = new Point(mouseState.X, mouseState.Y);
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                    lastDragPosition = default(Point);
                    draggingCamera = false;
                }
            }

            if (mouseState.ScrollWheelValue != lastScrollWheelValue)
            {
                originalSize = HandleCameraResize(board, mouseState);
            }

            board.Layout = new Layout(Layout.flat, originalSize, originPoint);

            //Update game state

            return true;
        }

        private ITile NewRandomTile(Hex newHex)
        {
            var randomBug = new Random(Guid.NewGuid().GetHashCode()).Next(0, 9);
            var randomTeam = new Random(Guid.NewGuid().GetHashCode()).Next(0, 2);

            switch (randomBug)
            {
                case 0:
                    return new BlankTile { Location = newHex };                    
                case 1:
                    return new Beetle(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 2:
                    return new Grasshopper(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 3:
                    return new LadyBug(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 4:
                    return new Mosquito(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 5:
                    return new PillBug(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 6:
                    return new QueenBee(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 7:
                    return new SoldierAnt(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
                case 8:
                    return new Spider(randomTeam == 0 ? BugTeam.Light : BugTeam.Dark) { Location = newHex };
            }
            return new BlankTile { Location = newHex };
        }

        private Point HandleCameraResize(Board board, MouseState mouseState)
        {
            var addedSize = (mouseState.ScrollWheelValue - lastScrollWheelValue) / 20;
            lastScrollWheelValue = mouseState.ScrollWheelValue;

            var newSize = addedSize + board.Layout.size.X;
            if (newSize > 60)
                newSize = 60;
            if (newSize < 30)
                newSize = 30;

            return new Point(newSize, newSize);
        }

        bool draggingCamera = false;
        Point lastDragPosition = default(Point);
        int lastScrollWheelValue = default(int);
        private Point HandleCameraDrag(Board board, MouseState mouseState)
        {
            Console.WriteLine($"MouseDrag: {lastDragPosition.X}, {lastDragPosition.Y}");
            var mouseDragChange = new Point(-1, -1) * (lastDragPosition - new Point(mouseState.X, mouseState.Y));
            Console.WriteLine($"MouseDragChange: {mouseDragChange.X}, {mouseDragChange.Y}");
            return board.Layout.origin + mouseDragChange;
        }
    }
}
