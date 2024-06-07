using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoChess.GUI
{
    interface IGUIElement
    {
        public void Draw(SpriteBatch spriteBatch);
        public virtual void Update(bool click) { }
    }
}
