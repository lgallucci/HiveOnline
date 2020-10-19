using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HiveLib.Bugs
{
    public class BlankTile : Tile
    {
        public BlankTile()
        {
            Type = BugType.Blank;
            Team = BugTeam.Blank;
        }

        public override bool BugCanMoveTo(Board board, Hex position)
        {
            return false;
        }

        public override Texture2D GetTexture()
        {
            throw new NotImplementedException();
        }

        public override void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, Board board)
        {
            //GraphicsDevice.SetRenderTarget(renderTarget);


            HiveContracts.Point firstPoint = new HiveContracts.Point(0, 0);
            Nullable<HiveContracts.Point> previousPoint = null;
            foreach (var corner in board.Layout.PolygonCorners(Location))
            {
                if (previousPoint.HasValue)
                {
                    spriteBatch.DrawLine(Art.Pixel, corner.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
                }
                else
                {
                    firstPoint = corner;
                }

                previousPoint = corner;
            }

            spriteBatch.DrawLine(Art.Pixel, firstPoint.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
            var vector2 = board.Layout.HexToPixel(Location);
            spriteBatch.DrawString(Art.ChatFont, $"{Location.q}, {Location.r}, {Location.s}",
                new Vector2((float)vector2.X - 20, (float)vector2.Y - 7), Color.Red);
        }
    }
}
