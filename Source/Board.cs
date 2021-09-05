using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.Models;

namespace MonoChess
{
    public class Board
    {
        readonly Piece[,] board = new Piece[8, 8];

        public Board()
        {
            SetInitialPlacement();
        }

        public Piece this[Position pos]
        {
            get => board[pos.X, pos.Y];
        }

        public void MakeMove(Move move)
        {
            MakeMove(move.Piece, move.Position);
        }

        public void MakeMove(Piece piece, Position pos)
        {
            var oldPos = piece.Position;
            piece.Position = pos;

            board[pos.X, pos.Y] = piece;
            board[oldPos.X, oldPos.Y] = new Piece();
        }

        public IEnumerable<Piece> GetPieces(Sides side)
        {
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    if (board[x, y].Side == side)
                    {
                        yield return board[x, y];
                    }
                }
            }
        }

        public IEnumerable<Move> GenerateMoves(Piece piece)
        {
            foreach (var dir in Piece.Directions[piece.Type])
            {
                var direction = piece.Side == Sides.Black ? dir * -1 : dir;

                var move = piece.Position - direction;

                while (move.X >= 0 && move.X <= 7 && move.Y >= 0 && move.Y <= 7)
                {
                    if (!board[move.X, move.Y].IsNull)
                    {
                        if (piece.Type == Pieces.Pawn && direction.Straight)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (board[move.X, move.Y].Side != piece.Side)
                        {
                            yield return new Move(piece, move);
                        }
                        break; //path blocked
                    }

                        if (piece.Type == Pieces.Pawn && !direction.Straight)
                        {
                            break; //prevent pawn lateral movement
                        }

                        yield return new Move(piece, move);

                    if (Piece.RangeLimited[piece.Type]) break;

                    move -= direction;
                }
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

            foreach (var side in new Sides[] { Sides.Black, Sides.White })
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
    }
}
