﻿using HiveClient;
using HiveContracts;
using HiveLib.Bugs;
using HiveOnline.Bugs;
using Microsoft.Xna.Framework.Input;
using System;

namespace HiveOnline
{
    class GameEngine
    {
        private HiveGameClient _hiveClient;
        private string _userName = string.Empty;
        private string _address = string.Empty;
        private string _key = string.Empty;

        internal bool Run()
        {
            //CONNNECT TO SERVER
            _hiveClient = new HiveGameClient(_address, _key, _userName);

            //if (!_hiveClient.Connect())
            //{
            //    return false;
            //}
            return true;
        }

        internal bool Update(ref IBoard board)
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
                    draggingCamera = true;
                    if (lastDragPosition == default(Point))
                        lastDragPosition = new Point(mouseState.X, mouseState.Y);

                    originPoint = HandleCameraDrag(board, mouseState);

                    lastDragPosition = new Point(mouseState.X, mouseState.Y);
                }
                else
                {
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

        private Point HandleCameraResize(IBoard board, MouseState mouseState)
        {
            var addedSize = (mouseState.ScrollWheelValue - lastScrollWheelValue) / 20;
            lastScrollWheelValue = mouseState.ScrollWheelValue;

            var newSize = addedSize + board.Layout.size.x;
            if (newSize > 60)
                newSize = 60;
            if (newSize < 30)
                newSize = 30;

            return new Point(newSize, newSize);
        }

        bool draggingCamera = false;
        Point lastDragPosition = default(Point);
        int lastScrollWheelValue = default(int);
        private Point HandleCameraDrag(IBoard board, MouseState mouseState)
        {
            Console.WriteLine($"MouseDrag: {lastDragPosition.x}, {lastDragPosition.y}");
            var mouseDragChange = new Point(-1, -1) * (lastDragPosition - new Point(mouseState.X, mouseState.Y));
            Console.WriteLine($"MouseDragChange: {mouseDragChange.x}, {mouseDragChange.y}");
            return board.Layout.origin + mouseDragChange;
        }
    }

    struct GameInputs
    {
        //public bool MouseClicked;
        //public Point MousePosition;
    }
}
