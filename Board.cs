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
    public class Board
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


        public Board(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, SpriteFont font)
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

            Fill();
        }

        public Piece Get(Position pos)
        {
            return board[pos.X, pos.Y];
        }

        public void Move(Piece piece, Position pos)
        {
            var oldPos = piece.Position;
            piece.Position = pos;

            var movedPiece = piece.Clone();

            board[pos.X, pos.Y] = movedPiece;
            board[oldPos.X, oldPos.Y] = null;
        }

        public void Update()
        {
            MouseState ms = Mouse.GetState();
            var pos = GetCursorPosition(ms.X, ms.Y);

            if (draggedPiece == null && ms.LeftButton == ButtonState.Pressed)
            {
                draggedPiece = board[pos.X, pos.Y];
            }

            if (draggedPiece != null && ms.LeftButton == ButtonState.Released)
            {
                if (board[pos.X, pos.Y] == null && allowedMoves.Contains(pos))
                {
                    Move(draggedPiece, pos);
                }

                draggedPiece = null;
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

                    if (board[x, y] != null)
                    {
                        if (draggedPiece != null && board[x, y] == draggedPiece)
                        {
                            continue;
                        }

                        rect.Height = (int)(rect.Height * 0.8f);
                        rect.Width = (int)(rect.Width * 0.7f);
                        rect.Y += (int)(rect.Height * 0.2f);
                        rect.X += (int)(rect.Width * 0.2f);

                        spriteBatch.Draw(textures[board[x, y].Name], rect, Color.White);
                    }
                    else if (draggedPiece != null && draggedPiece.MoveAllowed(new Position(x, y), board))
                    {
                        allowedMoves.Add(new(x, y));
                        spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
                    }
                }
            }

            if (draggedPiece != null)
            {
                rect = new(ms.X, ms.Y, (int)(size * 0.7f), (int)(size * 0.8f));
                rect.X -= (int)(0.5f * rect.Width);
                rect.Y -= (int)(0.5f * rect.Height);

                spriteBatch.Draw(textures[draggedPiece.Name], rect, Color.White);
            }
        }

        private void Fill()
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
