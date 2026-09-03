using HiveContracts;
using FontStashSharp;
using System.Collections.Generic;
using System.Text;

namespace HiveGraphics.GameAssetsDraw;
public class ChatBoxGraphics : DrawableObject
{
    private const int _textBoxHeight = 250;
    private const int _textBoxWidth = 500;
    private const int _textBuffer = 2;

    public void ChangeScreenSize(HexPoint ScreenSize)
    {
        Location = new Rectangle((int)ScreenSize.X - _textBoxWidth, (int)ScreenSize.Y - _textBoxHeight, _textBoxWidth - 5, _textBoxHeight - 5);
    }

    private readonly Dictionary<string, ChatLayout> _layoutCache = new Dictionary<string, ChatLayout>();

    public void Draw(string typingText, bool isTyping, IEnumerable<(string, int, string)> messages)
    {
        //DRAW BOX
        GraphicsEngine.SpriteBatch.Draw(Art.Pixel, Location, new Color(26, 50, 38));

        //DRAW LINE
        if (!string.IsNullOrWhiteSpace(typingText) || isTyping)
            typingText = $"> {typingText}_";
        else
            typingText = " ";

        var typingTextSize = Art.ChatFont.MeasureString(typingText);
        var lineHeight = Location.Bottom - typingTextSize.Y - _textBuffer;
        GraphicsEngine.SpriteBatch.DrawLine(Art.Pixel, new Vector2(Location.Left, lineHeight),
                             new Vector2(Location.Right, lineHeight),
                             Color.MintCream, 2f);

        //DRAW TYPING TEXT
        GraphicsEngine.SpriteBatch.DrawString(Art.ChatFont, typingText, new Vector2(Location.Left + _textBuffer, Location.Bottom - typingTextSize.Y - _textBuffer), Color.MintCream);

        int textHeight = 0;
        //DRAW SERVER TEXT
        foreach (var text in messages)
        {
            if (lineHeight - textHeight - 5 > Location.Top)
            {
                textHeight = DrawChatText(Art.ChatFont, text.Item1, text.Item2, text.Item3, textHeight, lineHeight);
            }
        }
    }

    public int DrawChatText(DynamicSpriteFont font, string playerName, int playerTeam, string message, int textHeight, float lineHeight)
    {
        var cacheKey = $"{playerName}\0{playerTeam}\0{message}\0{Location.Width}";
        if (!_layoutCache.TryGetValue(cacheKey, out var layout))
        {
            var nameOffset = font.MeasureString(playerName).X;
            var wrappedText = WrapText(font, $": {message}", _textBoxWidth - nameOffset - 5);
            layout = new ChatLayout(nameOffset, wrappedText, font.MeasureString(wrappedText).Y);
            _layoutCache[cacheKey] = layout;
        }

        textHeight += (int)layout.Height;

        GraphicsEngine.SpriteBatch.DrawString(font, playerName, new Vector2(Location.Left + _textBuffer, lineHeight - textHeight - 5), GetPlayerColor(playerTeam));

        GraphicsEngine.SpriteBatch.DrawString(font, layout.WrappedText, new Vector2(Location.Left + _textBuffer + layout.NameWidth, lineHeight - textHeight - 5), Color.MintCream);

        return textHeight;
    }

    private Color GetPlayerColor(int playerTeam)
    {
        if (playerTeam == 1)
            return Color.CornflowerBlue;
        return Color.Red;
    }

    public string WrapText(DynamicSpriteFont spriteFont, string text, float maxLineWidth)
    {
        string[] words = text.Split(' ');
        StringBuilder sb = new StringBuilder();
        float lineWidth = 0f;
        float spaceWidth = spriteFont.MeasureString(" ").X;

        foreach (string word in words)
        {
            Vector2 size = spriteFont.MeasureString(word);

            if (lineWidth + size.X < maxLineWidth)
            {
                sb.Append(word + " ");
                lineWidth += size.X + spaceWidth;
            }
            else
            {
                sb.Append("\n" + word + " ");
                lineWidth = size.X + spaceWidth;
            }
        }

        return sb.ToString();
    }

    private readonly struct ChatLayout
    {
        public ChatLayout(float nameWidth, string wrappedText, float height)
        {
            NameWidth = nameWidth;
            WrappedText = wrappedText;
            Height = height;
        }

        public float NameWidth { get; }
        public string WrappedText { get; }
        public float Height { get; }
    }
}
