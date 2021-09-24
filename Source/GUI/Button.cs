using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;


namespace MonoChess.GUI
{
    class Button : IGUIElement
    {
        public Action Action { get; set; }
        public Texture2D Texture { get; set; }
        public DynamicSpriteFont Font { get; set; }
        public Color TextColor { get; set; }
        public Rectangle Rect { get; set; }
        public string Text { get; set; }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rect, Color.White);

            if (Text != null)
            {
                var textPosition = new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2) - Font.MeasureString(Text) / 2;
                spriteBatch.DrawString(Font, Text, textPosition, TextColor);
            }
        }

        public void Update(Point mouseLoc, bool click)
        {
            if (Action != null && Rect.Contains(mouseLoc) && click)
            {
                Action();
            }
        }
    }
}