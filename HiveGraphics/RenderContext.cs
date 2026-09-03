using Microsoft.Xna.Framework.Graphics;

namespace HiveGraphics;

public sealed class RenderContext
{
    public SpriteBatch SpriteBatch { get; internal set; }
    public BloomRenderer Bloom { get; }

    public RenderContext(BloomRenderer bloom)
    {
        Bloom = bloom;
    }
}
