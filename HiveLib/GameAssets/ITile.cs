using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HiveLib.GameAssets
{
    public interface ITile
    {
        BugType Type { get; set; }
        BugTeam Team { get; set; }
        Hex Location { get; set; }

        bool CanMove(Board board);
        void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, Board board);
        void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, BloomFilter bloomFilter, Vector2 location, HiveContracts.Point tileSize);
    }
}
