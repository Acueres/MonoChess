using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace MonoChess
{
    public class Chess
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D whiteTile;
        Texture2D blackTile;
        Texture2D goldTile;
        SpriteFont font;
        Dictionary<string, Texture2D> textures;

        Board board = new();
        Piece draggedPiece;
        List<Position> allowedMoves = new();

        public Chess(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, SpriteFont font)
        {
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;
            this.textures = textures;
            this.font = font;

            whiteTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            Color[] data = new Color[50 * 50];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Beige;
            }
            whiteTile.SetData(data);

            blackTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Olive;
            }
            blackTile.SetData(data);

            goldTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Gold;
            }
            goldTile.SetData(data);
        }

        public void Update()
        {
            MouseState ms = Mouse.GetState();
            var pos = GetCursorPosition(ms.X, ms.Y);
            pos.X = Math.Clamp(pos.X, 0, 7);
            pos.Y = Math.Clamp(pos.Y, 0, 7);

            if (draggedPiece.IsNull && !board[pos].IsNull && ms.LeftButton == ButtonState.Pressed)
            {
                draggedPiece = board[pos]; //select piece to move
                foreach (var move in board.GenerateMoves(draggedPiece))
                {
                    allowedMoves.Add(move);
                }
            }

            if (!draggedPiece.IsNull && ms.LeftButton == ButtonState.Released)
            {
                if ((board[pos].IsNull || board[pos].Side != draggedPiece.Side) && allowedMoves.Contains(pos))
                {
                    board.Move(draggedPiece, pos);
                }

                draggedPiece = new Piece();
                allowedMoves.Clear();
            }
        }

        public void Draw()
        {
            Rectangle rect;
            MouseState ms = Mouse.GetState();
            var size = (int)(graphics.PreferredBackBufferWidth / 8f);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Position pos = new(x, y);
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                    spriteBatch.DrawString(font, x + " " + y, new Vector2(x * size, y * size), Color.Red);

                    if (!board[pos].IsNull)
                    {
                        if (!draggedPiece.IsNull && board[pos] == draggedPiece)
                        {
                            continue;
                        }

                        rect.Height = (int)(rect.Height * 0.8f);
                        rect.Width = (int)(rect.Width * 0.7f);
                        rect.Y += (int)(rect.Height * 0.2f);
                        rect.X += (int)(rect.Width * 0.2f);

                        spriteBatch.Draw(textures[board[pos].Name], rect, Color.White);
                    }
                }
            }

            //Draw dragged piece at cursor's position
            if (!draggedPiece.IsNull)
            {
                rect = new(ms.X, ms.Y, (int)(size * 0.7f), (int)(size * 0.8f));
                rect.X -= (int)(0.5f * rect.Width);
                rect.Y -= (int)(0.5f * rect.Height);

                spriteBatch.Draw(textures[draggedPiece.Name], rect, Color.White);

                rect = new(0, 0, size, size);
                foreach (var move in allowedMoves)
                {
                    rect.X = move.X * size;
                    rect.Y = move.Y * size;
                    spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
                }
            }
        }

        //Gets cursor's file and rank position
        Position GetCursorPosition(int x, int y)
        {
            var size = (int)(graphics.PreferredBackBufferWidth / 8f);
            var pos = new Position(x, y);
            return pos / size;
        }
    }
}
