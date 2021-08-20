using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SharpChess
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

        readonly Dictionary<Pieces, string> whiteFigures = new()
        {
            [Pieces.Pawn] = "♙",
            [Pieces.Knight] = "♘",
            [Pieces.Bishop] = "♗",
            [Pieces.Rook] = "♖",
            [Pieces.Queen] = "♕",
            [Pieces.King] = "♔"
        };

        readonly Dictionary<Pieces, string> blackFigures = new()
        {
            [Pieces.Pawn] = "♟",
            [Pieces.Knight] = "♞",
            [Pieces.Bishop] = "♝",
            [Pieces.Rook] = "♜",
            [Pieces.Queen] = "♛",
            [Pieces.King] = "♚"
        };

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

        public void Move(Piece piece, Position old, Position _new)
        {
            piece.Position = _new;
            board[_new.Rank, _new.File] = piece;
            board[old.Rank, old.File] = null;
        }

        public void Draw()
        {
            var size = (int)(graphics.PreferredBackBufferWidth / 8f);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    Rectangle rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

                    if (board[x, y] != null)
                    {
                        string side = board[x, y].Side == Sides.White ? "w" : "b";
                        string pieceType = board[x, y].Type.ToString().ToLower();
                        string pieceName = side + "_" + pieceType;

                        rect.Height = (int)(rect.Height * 0.8f);
                        rect.Width = (int)(rect.Width * 0.7f);
                        rect.Y += (int)(rect.Height * 0.2f);
                        rect.X += (int)(rect.Width * 0.2f);

                        spriteBatch.Draw(textures[pieceName], rect, Color.White);
                    }
                }
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
                int rank = side == Sides.Black ? 0 : 7;

                for (int file = 0; file < 8; file++)
                {
                    board[rank, file] = new Piece(arrangementOrder[file], side, new Position(rank, file));
                }

                if (side == Sides.Black)
                {
                    rank++;
                }
                else
                {
                    rank--;
                }

                for (int file = 0; file < 8; file++)
                {
                    board[rank, file] = new Piece(Pieces.Pawn, side, new Position(rank, file));
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
    }
}
