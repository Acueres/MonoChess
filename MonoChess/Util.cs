using MonoChess.Enums;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace MonoChess
{
    static class Util
    {
        public static Texture2D GetColoredTexture(GraphicsDevice graphics, int width, int height, Color color, float alpha = 1f)
        {
            var texture = new Texture2D(graphics, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Color(color, alpha);
            }
            texture.SetData(data);

            return texture;
        }

        public static bool MouseClicked(ButtonState currentState, ButtonState previousState)
        {
            return currentState == ButtonState.Pressed && previousState == ButtonState.Released;
        }

        public static bool KeyPressed(Keys key, KeyboardState currentState, KeyboardState previousState)
        {
            return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);
        }

        public static Side ReverseSide(Side side)
        {
            return side == Side.White ? Side.Black : Side.White;
        }
    }
}
