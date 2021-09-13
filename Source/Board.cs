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
        public Dictionary<Position, Piece> Copy { get => new(board); }
        readonly Dictionary<Position, Piece> board = new();

        public Board()
        {
            SetInitialPlacement();
        }

        public Board(Board board)
        {
            this.board = board.Copy;
        }

        public Piece this[Position pos]
        {
            get => board.ContainsKey(pos) ? board[pos] : new Piece();
            set => board.Add(pos, value);
        }

        public void MakeMove(Move move)
        {
            MakeMove(move.Piece, move.Position);
        }

        public void MakeMove(Move move, out Piece removed)
        {
            removed = MakeMove(move.Piece, move.Position);
        }

        public void ReverseMove(Move move, Piece removedPiece)
        {
            if (!removedPiece.IsNull)
            {
                board[move.Position] = removedPiece;
            }
            else
            {
                board.Remove(move.Position);
            }

            board.Add(move.Piece.Position, move.Piece);
        }

        public Piece MakeMove(Piece piece, Position pos)
        {
            Piece removed = new() { Position = pos };

            board.Remove(piece.Position);

            piece.Position = pos;

            if (board.ContainsKey(pos))
            {
                removed = board[pos];
                board[pos] = piece;
            }
            else
            {
                board.Add(pos, piece);
            }

            return removed;
        }

        public int GetScore(Sides side)
        {
            int res = 0;
            foreach (var piece in board.Values)
            {
                res += Piece.Scores[piece.Type] * (piece.Side == side ? 1 : -1);
            }

            return res;
        }

        public IEnumerable<Piece> GetPieces(Sides side)
        {
            foreach (var piece in board.Values.ToArray())
            {
                if (piece.Side == side)
                {
                    yield return piece;
                }
            }
        }

        public IEnumerable<Piece> GetPieces()
        {
            foreach (var piece in board.Values)
            {
                yield return piece;
            }
        }

        public IEnumerable<Move> GenerateMoves(Sides side)
        {
            foreach (var piece in GetPieces(side))
            {
                foreach (var move in GenerateMoves(piece))
                {
                    yield return move;
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
                    if (board.ContainsKey(move))
                    {
                        if (piece.Type == Pieces.Pawn && direction.Orthogonal)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (board[move].Side != piece.Side)
                        {
                            yield return new Move(piece, move);
                        }
                        break; //path blocked
                    }

                    if (piece.Type == Pieces.Pawn && !direction.Orthogonal)
                    {
                        break; //prevent pawn lateral movement
                    }

                    yield return new Move(piece, move);

                    if (Piece.RangeLimited[piece.Type])
                    {
                        //allow pawn to move two tiles from initial rank
                        if (piece.Type == Pieces.Pawn && !board.ContainsKey(move - direction) &&
                            ((piece.Side == Sides.White && piece.Position.Y == 6)) ||
                            (piece.Side == Sides.Black && piece.Position.Y == 1))
                        {
                            move -= direction;
                            yield return new Move(piece, move);
                        }
                        break;
                    }

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
                Pieces.Queen,
                Pieces.King,
                Pieces.Bishop,
                Pieces.Knight,
                Pieces.Rook
            };

            foreach (var side in new Sides[] { Sides.Black, Sides.White })
            {
                int y = side == Sides.Black ? 0 : 7;

                for (int x = 0; x < 8; x++)
                {
                    var pos = new Position(x, y);
                    board.Add(pos, new Piece(arrangementOrder[x], side, pos));
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
                    var pos = new Position(x, y);
                    board.Add(pos, new Piece(Pieces.Pawn, side, pos));
                }
            }
        }
    }
}
