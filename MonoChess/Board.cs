using System;
using System.Collections.Generic;
using System.Linq;

using MonoChess.Models;
using MonoChess.Enums;

namespace MonoChess
{
    public class Board
    {
        public const int SIZE = 504;
        const int BOARD_SIZE = 8;

        bool whiteCastling = true;
        bool blackCastling = true;

        readonly sbyte[] data = new sbyte[BOARD_SIZE * BOARD_SIZE];

        public Board()
        {
            SetPieces();
        }

        public Board(Piece[] piecesData)
        {
            SetPieces(piecesData);
        }

        public Board(Board board)
        {
            Array.Copy(board.data, data, BOARD_SIZE * BOARD_SIZE);
            board.SetCastlingData(whiteCastling, blackCastling);
        }

        public Piece this[Position pos]
        {
            get => new(data[pos.X + (pos.Y * BOARD_SIZE)], pos);
            set => data[pos.X + (pos.Y * BOARD_SIZE)] = value.Data;
        }

        public void MakeMove(Move move, out Piece removed)
        {
            if (move.CastlingCondition(this[move.TargetPosition]))
            {
                removed = CastlingMove(move.TargetPosition, move.Piece.Side);
            }
            else if (move.PromotionCondition())
            {
                removed = MakeMove(new Piece(Pieces.Queen, move.Piece.Side, move.Piece.Position), move.TargetPosition);
            }
            else
            {
                if (GetCastling(move.Piece.Side) && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece))
                {
                    SetCastling(move.Piece.Side, false);
                }

                removed = MakeMove(move.Piece, move.TargetPosition);
            }
        }

        private Piece MakeMove(Piece piece, Position targetPos)
        {
            this[piece.Position] = Piece.Null;

            piece.Position = targetPos;

            var removed = this[targetPos];
            this[targetPos] = piece;

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

            this[oldKingPos] = Piece.Null;
            this[oldRookPos] = Piece.Null;

            this[kingPos] = new Piece(Pieces.King, side, kingPos);
            this[rookPos] = new Piece(Pieces.Rook, side, rookPos);

            SetCastling(side, false);

            return rook;
        }

        public void ReverseMove(Move move, Piece removedPiece)
        {
            if (move.CastlingCondition(removedPiece))
            {
                ReverseCastlingMove(move.TargetPosition, move.Piece.Side);
                return;
            }
            else if (!GetCastling(move.Piece.Side) && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece))
            {
                SetCastling(move.Piece.Side, true);
            }

            this[move.TargetPosition] = removedPiece;
            this[move.Piece.Position] = move.Piece;
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

            this[oldKingPos] = Piece.Null;
            this[oldRookPos] = Piece.Null;

            this[kingPos] = new Piece(Pieces.King, side, kingPos);
            this[rookPos] = new Piece(Pieces.Rook, side, rookPos);

            SetCastling(side, true);
        }

        private bool CastlingCandidateMovement(Piece piece)
        {
            //piece is guaranteed to be either king or rook
            //piece is guaranteed to change its position
            Sides side = piece.Side;
            if (piece.Type == Pieces.King)
            {
                return true;
            }

            Span<Piece> rooks = stackalloc Piece[2];
            for (int i = 0, rooksIndex = 0; i < data.Length && rooksIndex < 2; i++)
            {
                if (data[i] == 0) continue;

                if (Piece.GetSide(data[i]) == side && Piece.GetType(data[i]) == Pieces.Rook)
                {
                    rooks[rooksIndex] = new Piece(data[i], new(i % BOARD_SIZE, i / BOARD_SIZE));
                    rooksIndex++;
                }
            }

            if (rooks[1].IsNull)
            {
                return true;
            }

            var otherRook = rooks[0] == piece ? rooks[1] : rooks[0];
            Position otherRookPosition;

            if (piece.Position.X == 0)
            {
                otherRookPosition = new(7, piece.Position.Y);
            }
            else
            {
                otherRookPosition = new(0, piece.Position.Y);
            }

            if (otherRook.Position != otherRookPosition)
            {
                return true;
            }

            return false;
        }

        public int GetScore(Sides side)
        {
            int res = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i], new(i % BOARD_SIZE, i / BOARD_SIZE));
                res += piece.Score * (piece.Side == side ? 1 : -1);
            }

            return res;
        }

        public IEnumerable<Piece> GetPieces(Sides side)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i], new(i % BOARD_SIZE, i / BOARD_SIZE));
                if (piece.Side == side)
                {
                    yield return piece;
                }
            }
        }

        public IEnumerable<Piece> GetPieces()
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i], new(i % BOARD_SIZE, i / BOARD_SIZE));
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
                var direction = dir * (int)piece.Side;

                var targetPos = piece.Position - direction;

                while (targetPos.InBounds(0, BOARD_SIZE))
                {
                    if (!this[targetPos].IsNull)
                    {
                        if (piece.Type == Pieces.Pawn && direction.Orthogonal)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (this[targetPos].Side != piece.Side)
                        {
                            yield return new Move(piece, targetPos);
                        }
                        break; //path blocked
                    }

                    if (piece.Type == Pieces.Pawn && !direction.Orthogonal)
                    {
                        break; //prevent pawn lateral movement
                    }

                    yield return new Move(piece, targetPos);

                    if (piece.RangeLimited)
                    {
                        //allow pawn to move two tiles from initial rank
                        if (piece.Type == Pieces.Pawn && (this[targetPos - direction].IsNull) &&
                            ((piece.Side == Sides.White && piece.Position.Y == 6) ||
                            (piece.Side == Sides.Black && piece.Position.Y == 1)))
                        {
                            targetPos -= direction;
                            yield return new Move(piece, targetPos);
                        }
                        break;
                    }

                    targetPos -= direction;
                }
            }

            if (piece.Type == Pieces.King && GetCastling(piece.Side))
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
            var oppositeSide = Util.ReverseSide(side);
            var king = GetKing(side);
            if (king.IsNull) return true;

            foreach (var move in GenerateMoves(oppositeSide))
            {
                if (move.TargetPosition == king.Position)
                {
                    return true;
                }
            }

            return false;
        }

        public bool DetectCheck(Sides side, Move move)
        {
            var oppositeSide = Util.ReverseSide(side);
            MakeMove(move, out var removed);

            var king = GetKing(side);
            if (king.IsNull)
            {
                ReverseMove(move, removed);
                return true;
            }

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
            //returns false if there is at least one escape move
            foreach (var move in GenerateMoves(side))
            {
                if (!DetectCheck(side, move))
                {
                    return false;
                }
            }

            return true;
        }

        public Piece GetKing(Sides side)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                if (Piece.GetType(data[i]) == Pieces.King && Piece.GetSide(data[i]) == side)
                {
                    return new Piece(data[i], new(i % BOARD_SIZE, i / BOARD_SIZE));
                }
            }
            return Piece.Null;
        }

        public void SetCastlingData(bool whiteCastling, bool blackCastling)
        {
            this.whiteCastling = whiteCastling;
            this.blackCastling = blackCastling;
        }

        public bool[] GetCastlingData()
        {
            return new bool[] { whiteCastling, blackCastling };
        }

        public void SetCastling(Sides side, bool value)
        {
            if (side == Sides.White)
            {
                whiteCastling = value;
            }
            else
            {
                blackCastling = value;
            }
        }

        public bool GetCastling(Sides side)
        {
            if (side == Sides.White)
                return whiteCastling;

            return blackCastling;
        }

        public void SetPieces()
        {
            Array.Clear(data);

            SetCastlingData(true, true);

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

            Span<Sides> sides = stackalloc Sides[] { Sides.Black, Sides.White };
            foreach (var side in sides)
            {
                int y = side == Sides.Black ? 0 : 7;

                for (int x = 0; x < 8; x++)
                {
                    var pos = new Position(x, y);
                    this[pos] = new Piece(arrangementOrder[x], side, pos);
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
                    this[pos] = new Piece(Pieces.Pawn, side, pos);
                }
            }
        }

        public void SetPieces(int[] numData)
        {
            var piecesData = new List<Piece>();

            for (int y = 0, i = 0; y < 8 && i < numData.Length; y++)
            {
                for (int x = 0; x < 8 && i < numData.Length; x++, i++)
                {
                    if (numData[i] != 0)
                    {
                        piecesData.Add(new((Pieces)Math.Abs(numData[i]), numData[i] > 0 ? Sides.White : Sides.Black, new(x, y)));
                    }
                }
            }

            SetPieces(piecesData.ToArray());
        }

        private void SetPieces(Piece[] piecesData)
        {
            Array.Clear(data);

            foreach (var piece in piecesData)
            {
                this[piece.Position] = piece;
            }
        }

        public int[] GetPiecesData()
        {
            return data.Select(b => (int)b).ToArray();
        }
    }
}
