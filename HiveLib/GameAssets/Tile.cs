using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HiveOnline.GameAssets
{
    public abstract class Tile : ITile
    {
        public BugType Type { get; set; }
        public Hex Location { get; set; }
        public BugTeam Team { get; set; }

        public bool CanMoveTo(Board board, Hex position)
        {
            return CanMove(board) && BugCanMoveTo(board, position);
        }

        public virtual bool CanMove(Board board)
        {
            return true;
        }

        public virtual void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, Board board)
        {
            Draw(graphics, spriteBatch, bloomFilter, board.Layout.HexToPixel(Location).ToVector2(), board.Layout.size * 2);
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, Vector2 location, HiveContracts.Point tileSize)
        {
            var texture = GetTexture();
            spriteBatch.Draw(
                texture,
                location,
                new Rectangle(0, 0, texture.Width, texture.Height),
                Color.White,
                0,
                new Vector2((float)texture.Width / 2, (float)texture.Width / 2),
                new Vector2((float)tileSize.X / (texture.Width), (float)tileSize.Y / (texture.Height)), //Scale
                SpriteEffects.None, 0f);
        }

        public abstract bool BugCanMoveTo(Board board, Hex position);
        public abstract Texture2D GetTexture();
    }
}
