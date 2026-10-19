using HiveContracts;
using HiveLib.GameAssets;
using FontStashSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveGraphics.GameAssetsDraw;
public class ChatBoxGraphics : DrawableObject
{
    private const int _textBoxHeight = 250;
    private const int _textBoxWidth = 500;
    private const int _textBuffer = 2;

    private const int _headerHeight = 24;
    private HexPoint _screenSize;

    public void ChangeScreenSize(HexPoint ScreenSize)
    {
        _screenSize = ScreenSize;
        Location = new Rectangle((int)ScreenSize.X - _textBoxWidth, (int)ScreenSize.Y - _textBoxHeight, _textBoxWidth - 5, _textBoxHeight - 5);
    }

    private readonly Dictionary<string, ChatLayout> _layoutCache = new Dictionary<string, ChatLayout>();

    public void Draw(string typingText, bool isTyping, IEnumerable<ChatMessage> messages, bool collapsed = false, bool hasUnread = false)
    {
        var height = collapsed ? _headerHeight + 5 : _textBoxHeight;
        Location = new Rectangle((int)_screenSize.X - _textBoxWidth, (int)_screenSize.Y - height, _textBoxWidth - 5, height - 5);

        //DRAW GLOW (only while collapsed with an unread message)
        if (collapsed && hasUnread)
        {
            var glowColor = new Color(140, 220, 170);
            const int layers = 8;
            for (var i = layers; i >= 1; i--)
            {
                var alpha = 0.35f * (layers - i + 1) / layers;
                var glowRect = new Rectangle(Location.X - i, Location.Y - i, Location.Width + i * 2, Location.Height + i * 2);
                Context.SpriteBatch.Draw(Art.Pixel, glowRect, glowColor * (alpha * 0.5f));
            }
        }

        //DRAW BOX
        Context.SpriteBatch.Draw(Art.Pixel, Location, new Color(26, 50, 38));

        //DRAW OUTLINE
        if (collapsed && hasUnread)
        {
            var outline = new Color(140, 220, 170);
            Context.SpriteBatch.Draw(Art.Pixel, new Rectangle(Location.X, Location.Y, Location.Width, 2), outline);
            Context.SpriteBatch.Draw(Art.Pixel, new Rectangle(Location.X, Location.Bottom - 2, Location.Width, 2), outline);
            Context.SpriteBatch.Draw(Art.Pixel, new Rectangle(Location.X, Location.Y, 2, Location.Height), outline);
            Context.SpriteBatch.Draw(Art.Pixel, new Rectangle(Location.Right - 2, Location.Y, 2, Location.Height), outline);
        }

        //DRAW COLLAPSE BUTTON
        var caret = collapsed ? "^" : "v";
        var buttonRect = new Rectangle(Location.Left, Location.Top, _headerHeight, _headerHeight);
        Context.SpriteBatch.Draw(Art.Pixel, buttonRect, new Color(60, 100, 80));
        var caretSize = Art.ChatFont.MeasureString(caret);
        Context.SpriteBatch.DrawString(Art.ChatFont, caret,
            new Vector2(buttonRect.Left + (_headerHeight - caretSize.X) / 2, buttonRect.Top + (_headerHeight - caretSize.Y) / 2), Color.MintCream);

        if (collapsed)
        {
            Context.SpriteBatch.DrawString(Art.ChatFont, "Chat", new Vector2(buttonRect.Right + 6, buttonRect.Top + 2), Color.MintCream);
            return;
        }

        //DRAW LINE
        if (!string.IsNullOrWhiteSpace(typingText) || isTyping)
            typingText = $"> {typingText}_";
        else
            typingText = " ";

        var typingTextSize = Art.ChatFont.MeasureString(typingText);
        var lineHeight = Location.Bottom - typingTextSize.Y - _textBuffer;
        Context.SpriteBatch.DrawLine(Art.Pixel, new Vector2(Location.Left, lineHeight),
                             new Vector2(Location.Right, lineHeight),
                             Color.MintCream, 2f);

        //DRAW TYPING TEXT
        Context.SpriteBatch.DrawString(Art.ChatFont, typingText, new Vector2(Location.Left + _textBuffer, Location.Bottom - typingTextSize.Y - _textBuffer), Color.MintCream);

        int textHeight = 0;
        //DRAW SERVER TEXT
        foreach (var text in messages)
        {
            if (lineHeight - textHeight - 5 > Location.Top + _headerHeight)
            {
                textHeight = DrawChatText(Art.ChatFont, text.PlayerName, text.PlayerTeam, text.Message, textHeight, lineHeight);
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

        Context.SpriteBatch.DrawString(font, playerName, new Vector2(Location.Left + _textBuffer, lineHeight - textHeight - 5), GetPlayerColor(playerTeam));

        Context.SpriteBatch.DrawString(font, layout.WrappedText, new Vector2(Location.Left + _textBuffer + layout.NameWidth, lineHeight - textHeight - 5), Color.MintCream);

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
