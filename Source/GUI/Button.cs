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

        public static Texture2D BaseTexture { get; set; }
        public static Texture2D Highlight { get; set; }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture ?? BaseTexture, Rect, Color.White);

            if (Text != null)
            {
                var textPosition = new Vector2(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2) - Font.MeasureString(Text) / 2;
                spriteBatch.DrawString(Font, Text, textPosition, TextColor);
            }

            if (Highlight != null && Rect.Contains(Mouse.GetState().Position))
            {
                spriteBatch.Draw(Highlight, Rect, Color.White * 0.2f);
            }
        }

        public void Update(bool click)
        {
            if (Action != null && Rect.Contains(Mouse.GetState().Position) && click)
            {
                Action();
            }
        }
    }
}