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
        public const int SIZE = 504;

        Dictionary<Position, Piece> pieces = new();
        readonly bool[] castlingPossible = new bool[2] { true, true };

        public void Reset() => SetInitialPlacement();

        public Board()
        {
            SetInitialPlacement();
        }

        public Board(Board board)
        {
            pieces = new(board.pieces);
            Array.Copy(castlingPossible, board.castlingPossible, 2);
        }

        public Piece this[Position pos]
        {
            get => pieces.ContainsKey(pos) ? pieces[pos] : Piece.Null;
            set => pieces.Add(pos, value);
        }

        public Piece GetKing(Sides side)
        {
            return pieces.Values.SingleOrDefault(p => p.Type == Pieces.King && p.Side == side);
        }

        public void MakeMove(Move move, out Piece removed)
        {
            if (move.CastlingCondition(this[move.TargetPosition]))
            {
                removed = CastlingMove(move.TargetPosition, move.Piece.Side);
            }
            else
            {
                removed = MakeMove(move.Piece, move.TargetPosition);
            }
        }

        public void ReverseMove(Move move, Piece removedPiece)
        {
            if (move.CastlingCondition(removedPiece))
            {
                ReverseCastlingMove(move.TargetPosition, move.Piece.Side);
                return;
            }

            if (!removedPiece.IsNull)
            {
                pieces[move.TargetPosition] = removedPiece;
            }
            else
            {
                pieces.Remove(move.TargetPosition);
            }

            pieces.Add(move.Piece.Position, move.Piece);
        }

        private void ReverseCastlingMove(Position rookPos, Sides side)
        {
            Position oldRookPos;
            Position oldKingPos;

            Position kingPos = side == Sides.White ? new(4, 7) : new(4, 0);

            int rank = rookPos.X;
            int file = rookPos.Y;

            if (rank == 0)
            {
                oldRookPos = new(3, file);
                oldKingPos = new(2, file);
            }
            else
            {
                oldRookPos = new(5, file);
                oldKingPos = new(6, file);
            }

            pieces.Remove(oldKingPos);
            pieces.Remove(oldRookPos);

            pieces.Add(kingPos, new Piece(Pieces.King, side, kingPos));
            pieces.Add(rookPos, new Piece(Pieces.Rook, side, rookPos));

            castlingPossible[(int)side] = true;
        }

        public Piece MakeMove(Piece piece, Position pos)
        {
            Piece removed = new() { Position = pos }; //placeholder in case there is no piece at position

            pieces.Remove(piece.Position);

            piece.Position = pos;

            if (pieces.ContainsKey(pos))
            {
                removed = pieces[pos];
                pieces[pos] = piece;
            }
            else
            {
                pieces.Add(pos, piece);
            }

            return removed;
        }

        private Piece CastlingMove(Position oldRookPos, Sides side)
        {
            Piece rook = this[oldRookPos];

            Position rookPos;
            Position kingPos;

            Position oldKingPos = side == Sides.White ? new(4, 7) : new(4, 0);

            int rank = oldRookPos.X;
            int file = oldRookPos.Y;

            if (rank == 0)
            {
                rookPos = new(3, file);
                kingPos = new(2, file);
            }
            else
            {
                rookPos = new(5, file);
                kingPos = new(6, file);
            }

            pieces.Remove(oldKingPos);
            pieces.Remove(oldRookPos);

            pieces.Add(kingPos, new Piece(Pieces.King, side, kingPos));
            pieces.Add(rookPos, new Piece(Pieces.Rook, side, rookPos));

            castlingPossible[(int)side] = false;

            return rook;
        }

        public int GetScore(Sides side)
        {
            int res = 0;
            foreach (var piece in pieces.Values)
            {
                res += piece.Score * (piece.Side == side ? 1 : -1);
            }

            return res;
        }

        public IEnumerable<Piece> GetPieces(Sides side)
        {
            foreach (var piece in pieces.Values.ToArray())
            {
                if (piece.Side == side)
                {
                    yield return piece;
                }
            }
        }

        public IEnumerable<Piece> GetPieces()
        {
            foreach (var piece in pieces.Values)
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
            foreach (var dir in piece.Directions)
            {
                var direction = piece.Side == Sides.Black ? dir * -1 : dir;

                var move = piece.Position - direction;

                while (move.X >= 0 && move.X <= 7 && move.Y >= 0 && move.Y <= 7)
                {
                    if (pieces.ContainsKey(move))
                    {
                        if (piece.Type == Pieces.Pawn && direction.Orthogonal)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (pieces[move].Side != piece.Side)
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

                    if (piece.RangeLimited)
                    {
                        //allow pawn to move two tiles from initial rank
                        if (piece.Type == Pieces.Pawn && !pieces.ContainsKey(move - direction) &&
                            ((piece.Side == Sides.White && piece.Position.Y == 6) ||
                            (piece.Side == Sides.Black && piece.Position.Y == 1)))
                        {
                            move -= direction;
                            yield return new Move(piece, move);
                        }
                        break;
                    }

                    move -= direction;
                }
            }

            if (piece.Type == Pieces.King && castlingPossible[(int)piece.Side])
            {
                foreach (var castlingMove in GenerateCastlingMoves(piece))
                {
                    yield return castlingMove;
                }
            }
        }

        private IEnumerable<Move> GenerateCastlingMoves(Piece piece)
        {
            Position kingOrigin = piece.Side == Sides.White ? new(4, 7) : new(4, 0);
            Position rook1Origin = piece.Side == Sides.White ? new(0, 7) : new(0, 0);
            Position rook2Origin = piece.Side == Sides.White ? new(7, 7) : new(7, 0);

            if (piece.Position == kingOrigin
                && this[rook1Origin].Type == Pieces.Rook
                && this[rook1Origin].Side == piece.Side)
            {
                Position direction = new(-1, 0);

                bool pathBlocked = false;
                for (int i = 1; i <= 3; i++)
                {
                    if (!this[kingOrigin + direction * i].IsNull)
                    {
                        pathBlocked = true;
                        break;
                    }
                }

                if (!pathBlocked)
                {
                    yield return new Move(piece, rook1Origin);
                }
            }

            if (piece.Position == kingOrigin
                && this[rook2Origin].Type == Pieces.Rook
                && this[rook2Origin].Side == piece.Side)
            {
                Position direction = new(1, 0);

                bool pathBlocked = false;
                for (int i = 1; i <= 2; i++)
                {
                    if (!this[kingOrigin + direction * i].IsNull)
                    {
                        pathBlocked = true;
                        break;
                    }
                }

                if (!pathBlocked)
                {
                    yield return new Move(piece, rook2Origin);
                }
            }
        }

        public bool DetectCheck(Sides side)
        {
            var oppositeSide = side == Sides.White ? Sides.Black : Sides.White;
            var king = GetKing(side);
            if (king.IsNull) return true;

            foreach (var oppositeMove in GenerateMoves(oppositeSide))
            {
                if (oppositeMove.TargetPosition == king.Position)
                {
                    return true;
                }
            }

            return false;
        }

        public bool DetectCheck(Sides side, Move move)
        {
            var oppositeSide = side == Sides.White ? Sides.Black : Sides.White;
            MakeMove(move, out var removed);
            var king = GetKing(side);
            if (king.IsNull) return true;

            foreach (var oppositeMove in GenerateMoves(oppositeSide))
            {
                if (oppositeMove.TargetPosition == king.Position)
                {
                    ReverseMove(move, removed);
                    return true;
                }
            }

            ReverseMove(move, removed);
            return false;
        }

        public bool DetectCheckmate(Sides side)
        {
            foreach (var move in GenerateMoves(side))
            {
                if (!DetectCheck(side, move))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetInitialPlacement()
        {
            pieces.Clear();

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
                    pieces.Add(pos, new Piece(arrangementOrder[x], side, pos));
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
                    pieces.Add(pos, new Piece(Pieces.Pawn, side, pos));
                }
            }

            //ensuring the ai chooses more different types of pieces
            pieces = pieces.OrderBy(x => new Random().Next()).ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
