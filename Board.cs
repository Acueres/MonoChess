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
        SpriteFont font;
        Dictionary<string, Texture2D> textures;

        Piece[,] board = new Piece[8, 8];
        Piece draggedPiece;


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

            Fill();
        }

        public Piece Get(Position pos)
        {
            return board[pos.Rank, pos.File];
        }

        public void Move(Piece piece, Position pos)
        {
            var oldPos = piece.Position;
            piece.Position = pos;

            var movedPiece = piece.Clone();

            board[pos.Rank, pos.File] = movedPiece;
            board[oldPos.Rank, oldPos.File] = null;
        }

        public void Update()
        {
            MouseState ms = Mouse.GetState();
            var pos = GetCursorPosition(ms.X, ms.Y);

            if (draggedPiece == null && ms.LeftButton == ButtonState.Pressed)
            {
                draggedPiece = board[pos.Rank, pos.File];
            }

            if (draggedPiece != null && ms.LeftButton == ButtonState.Released)
            {
                if (board[pos.Rank, pos.File] == null)
                {
                    Move(draggedPiece, pos);
                }

                draggedPiece = null;
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
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

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
                int file = side == Sides.Black ? 0 : 7;

                for (int rank = 0; rank < 8; rank++)
                {
                    board[file, rank] = new Piece(arrangementOrder[rank], side, new Position(file, rank));
                }

                if (side == Sides.Black)
                {
                    file++;
                }
                else
                {
                    file--;
                }

                for (int rank = 0; rank < 8; rank++)
                {
                    board[file, rank] = new Piece(Pieces.Pawn, side, new Position(rank, file));
                }
            }

            board = Transpose(board);
        }

        private static Piece[,] Transpose(Piece[,] matrix)
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);

            var result = new Piece[columns, rows];

            for (var c = 0; c < columns; c++)
            {
                for (var r = 0; r < rows; r++)
                {
                    result[c, r] = matrix[r, c];
                }
            }

            return result;
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
