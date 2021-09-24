using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FontStashSharp;


namespace MonoChess.GUI
{
    class Label : IGUIElement
    {
        public string Text { get; set; }
        public DynamicSpriteFont Font { get; set; }
        public Texture2D Texture { get; set; }
        public Rectangle Rect { get; set; }
        public Color TextColor { get; set; }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Rect, Color.White);
            }

            if (Text != null)
            {
                var textPosition = new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 4) - Font.MeasureString(Text) / 2;
                spriteBatch.DrawString(Font, Text, textPosition, TextColor);
            }
        }
    }
}
