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

        Piece[,] board = new Piece[8, 8];
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

            SetInitialPlacement();
        }

        public void Move(Piece piece, Position pos)
        {
            var oldPos = piece.Position;
            piece.Position = pos;

            board[pos.X, pos.Y] = piece;
            board[oldPos.X, oldPos.Y] = new Piece();
        }

        public void Update()
        {
            MouseState ms = Mouse.GetState();
            var pos = GetCursorPosition(ms.X, ms.Y);

            if (draggedPiece.IsNull && ms.LeftButton == ButtonState.Pressed)
            {
                draggedPiece = board[pos.X, pos.Y];
            }

            if (!draggedPiece.IsNull && ms.LeftButton == ButtonState.Released)
            {
                if ((board[pos.X, pos.Y].IsNull || board[pos.X, pos.Y].Side != draggedPiece.Side) && allowedMoves.Contains(pos))
                {
                    Move(draggedPiece, pos);
                }

                draggedPiece = new Piece();
            }

            allowedMoves.Clear();
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
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                    spriteBatch.DrawString(font, x + " " + y, new Vector2(x * size, y * size), Color.Red);

                    if (!board[x, y].IsNull)
                    {
                        if (!draggedPiece.IsNull && board[x, y] == draggedPiece)
                        {
                            continue;
                        }

                        rect.Height = (int)(rect.Height * 0.8f);
                        rect.Width = (int)(rect.Width * 0.7f);
                        rect.Y += (int)(rect.Height * 0.2f);
                        rect.X += (int)(rect.Width * 0.2f);

                        spriteBatch.Draw(textures[board[x, y].Name], rect, Color.White);
                    }

                    if (draggedPiece.IsNull) continue;

                    //Move if tile is empty
                    if (board[x, y].IsNull && draggedPiece.MoveAllowed(new Position(x, y), board))
                    { 
                        allowedMoves.Add(new(x, y));
                        spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
                        continue;
                    }

                    //Attack
                    if (!board[x, y].IsNull && board[x, y].Side != draggedPiece.Side && draggedPiece.AttackAllowed(new Position(x, y), board))
                    {
                        allowedMoves.Add(new(x, y));
                        spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
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
            }
        }

        private void SetInitialPlacement()
        {
            var arrangementOrder = new Pieces[]
            {
                Pieces.Rook,
                Pieces.Knight,
                Pieces.Bishop,
                Pieces.King,
                Pieces.Queen,
                Pieces.Bishop,
                Pieces.Knight,
                Pieces.Rook
            };

            foreach (var side in new Sides[] { Sides.Black, Sides.White})
            {
                int y = side == Sides.Black ? 0 : 7;

                for (int x = 0; x < 8; x++)
                {
                    board[x, y] = new Piece(arrangementOrder[x], side, new Position(x, y));
                }

                if (side == Sides.Black)
                {
                    y++;
                }
                else
                {
                    y--;
                }

                for (int x = 0; x < 8; x++)
                {
                    board[x, y] = new Piece(Pieces.Pawn, side, new Position(x, y));
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
