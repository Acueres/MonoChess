using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoChess.Components
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

        public Board(IEnumerable<(Piece, Position)> piecesData)
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
            get => new(data[pos.X + pos.Y * BOARD_SIZE]);
            set => data[pos.X + pos.Y * BOARD_SIZE] = value.Value;
        }

        public void MakeMove(Move move, out Piece removed)
        {
            if (move.CastlingCondition(this[move.TargetPosition]))
            {
                removed = CastlingMove(move.TargetPosition, move.Piece.Side);
            }
            else if (move.PromotionCondition())
            {
                removed = MakeMove(new Piece(PieceType.Queen, move.Piece.Side), move.CurrentPosition, move.TargetPosition);
            }
            else
            {
                if (GetCastling(move.Piece.Side) && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece, move.CurrentPosition))
                {
                    SetCastling(move.Piece.Side, false);
                }

                removed = MakeMove(move.Piece, move.CurrentPosition, move.TargetPosition);
            }
        }

        private Piece MakeMove(Piece piece, Position currentPosition, Position targetPos)
        {
            this[currentPosition] = Piece.Null;

            var removed = this[targetPos];
            this[targetPos] = piece;

            return removed;
        }

        private Piece CastlingMove(Position oldRookPos, Side side)
        {
            Piece rook = this[oldRookPos];

            Position rookPos;
            Position kingPos;

            Position oldKingPos = side == Side.White ? new(4, 7) : new(4, 0);

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

            this[kingPos] = new Piece(PieceType.King, side);
            this[rookPos] = new Piece(PieceType.Rook, side);

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
            else if (!GetCastling(move.Piece.Side) && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece, move.CurrentPosition))
            {
                SetCastling(move.Piece.Side, true);
            }

            this[move.TargetPosition] = removedPiece;
            this[move.CurrentPosition] = move.Piece;
        }

        private void ReverseCastlingMove(Position rookPos, Side side)
        {
            Position oldRookPos;
            Position oldKingPos;

            Position kingPos = side == Side.White ? new(4, 7) : new(4, 0);

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

            this[kingPos] = new Piece(PieceType.King, side);
            this[rookPos] = new Piece(PieceType.Rook, side);

            SetCastling(side, true);
        }

        private bool CastlingCandidateMovement(Piece piece, Position pos)
        {
            //piece is guaranteed to be either king or rook
            //piece is guaranteed to change its position
            Side side = piece.Side;
            if (piece.Type == PieceType.King)
            {
                return true;
            }

            Span<Position> rookPositions = stackalloc Position[2];
            for (int i = 0, rooksIndex = 0; i < data.Length && rooksIndex < 2; i++)
            {
                if (data[i] == 0) continue;

                Piece p = new(data[i]);
                if (p.Side == side && p.Type == PieceType.Rook)
                {
                    rookPositions[rooksIndex] = new(i % BOARD_SIZE, i / BOARD_SIZE);
                    rooksIndex++;
                }
            }

            if (rookPositions[1].IsNull)
            {
                return true;
            }

            var otherRook = rookPositions[0] == pos ? rookPositions[1] : rookPositions[0];
            Position otherRookPosition;

            if (pos.X == 0)
            {
                otherRookPosition = new(7, pos.Y);
            }
            else
            {
                otherRookPosition = new(0, pos.Y);
            }

            if (otherRook != otherRookPosition)
            {
                return true;
            }

            return false;
        }

        public int GetScore(Side side)
        {
            int res = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i]);
                res += piece.Score * (piece.Side == side ? 1 : -1);
            }

            return res;
        }

        public IEnumerable<(Piece, Position)> GetPieces(Side side)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i]);
                if (piece.Side == side)
                {
                    yield return (piece, new(i % BOARD_SIZE, i / BOARD_SIZE));
                }
            }
        }

        public IEnumerable<(Piece, Position)> GetPieces()
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece piece = new(data[i]);
                yield return (piece, new(i % BOARD_SIZE, i / BOARD_SIZE));
            }
        }

        public IEnumerable<Move> GenerateMoves(Side side)
        {
            foreach (var (piece, pos) in GetPieces(side))
            {
                foreach (var move in GenerateMoves(piece, pos))
                {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> GenerateMoves(Piece piece, Position pos)
        {
            foreach (var dir in piece.Directions)
            {
                var direction = dir * (int)piece.Side;

                var targetPos = pos - direction;

                while (targetPos.InBounds(0, BOARD_SIZE))
                {
                    if (!this[targetPos].IsNull)
                    {
                        if (piece.Type == PieceType.Pawn && direction.Orthogonal)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (this[targetPos].Side != piece.Side)
                        {
                            yield return new Move(piece, pos, targetPos);
                        }
                        break; //path blocked
                    }

                    if (piece.Type == PieceType.Pawn && !direction.Orthogonal)
                    {
                        break; //prevent pawn lateral movement
                    }

                    yield return new Move(piece, pos, targetPos);

                    if (piece.IsRangeLimited)
                    {
                        //allow pawn to move two tiles from initial rank
                        if (piece.Type == PieceType.Pawn && this[targetPos - direction].IsNull &&
                            (piece.Side == Side.White && pos.Y == 6 ||
                            piece.Side == Side.Black && pos.Y == 1))
                        {
                            targetPos -= direction;
                            yield return new Move(piece, pos, targetPos);
                        }
                        break;
                    }

                    targetPos -= direction;
                }
            }

            if (piece.Type == PieceType.King && GetCastling(piece.Side))
            {
                foreach (var castlingMove in GenerateCastlingMoves(piece, pos))
                {
                    yield return castlingMove;
                }
            }
        }

        private IEnumerable<Move> GenerateCastlingMoves(Piece piece, Position pos)
        {
            Position kingOrigin = piece.Side == Side.White ? new(4, 7) : new(4, 0);
            Position rook1Origin = piece.Side == Side.White ? new(0, 7) : new(0, 0);
            Position rook2Origin = piece.Side == Side.White ? new(7, 7) : new(7, 0);

            if (pos == kingOrigin
                && this[rook1Origin].Type == PieceType.Rook
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
                    yield return new Move(piece, pos, rook1Origin);
                }
            }

            if (pos == kingOrigin
                && this[rook2Origin].Type == PieceType.Rook
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
                    yield return new Move(piece, pos, rook2Origin);
                }
            }
        }

        public bool DetectCheck(Side side)
        {
            var oppositeSide = Util.ReverseSide(side);
            var pos = GetKingPosition(side);
            if (pos.IsNull) return true;

            foreach (var move in GenerateMoves(oppositeSide))
            {
                if (move.TargetPosition == pos)
                {
                    return true;
                }
            }

            return false;
        }

        public bool DetectCheck(Side side, Move move)
        {
            var oppositeSide = Util.ReverseSide(side);
            MakeMove(move, out var removed);

            var pos = GetKingPosition(side);
            if (pos.IsNull)
            {
                ReverseMove(move, removed);
                return true;
            }

            foreach (var oppositeMove in GenerateMoves(oppositeSide))
            {
                if (oppositeMove.TargetPosition == pos)
                {
                    ReverseMove(move, removed);
                    return true;
                }
            }

            ReverseMove(move, removed);

            return false;
        }

        public bool DetectCheckmate(Side side)
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

        public Position GetKingPosition(Side side)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0) continue;

                Piece p = new(data[i]);
                if (p.Side == side && p.Type == PieceType.King)
                {
                    return new(i % BOARD_SIZE, i / BOARD_SIZE);
                }
            }

            return Position.Null;
        }

        public void SetCastlingData(bool whiteCastling, bool blackCastling)
        {
            this.whiteCastling = whiteCastling;
            this.blackCastling = blackCastling;
        }

        public bool[] GetCastlingData()
        {
            return [whiteCastling, blackCastling];
        }

        public void SetCastling(Side side, bool value)
        {
            if (side == Side.White)
            {
                whiteCastling = value;
            }
            else
            {
                blackCastling = value;
            }
        }

        public bool GetCastling(Side side)
        {
            if (side == Side.White)
                return whiteCastling;

            return blackCastling;
        }

        public void SetPieces()
        {
            Array.Clear(data);

            SetCastlingData(true, true);

            var arrangementOrder = new PieceType[]
            {
                PieceType.Rook,
                PieceType.Knight,
                PieceType.Bishop,
                PieceType.Queen,
                PieceType.King,
                PieceType.Bishop,
                PieceType.Knight,
                PieceType.Rook
            };

            Span<Side> sides = [Side.Black, Side.White];
            foreach (var side in sides)
            {
                int y = side == Side.Black ? 0 : 7;

                for (int x = 0; x < 8; x++)
                {
                    var pos = new Position(x, y);
                    this[pos] = new Piece(arrangementOrder[x], side);
                }

                if (side == Side.Black)
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
                    this[pos] = new Piece(PieceType.Pawn, side);
                }
            }
        }

        public void SetPieces(int[] numData)
        {
            var piecesData = new List<(Piece, Position)>();

            for (int y = 0, i = 0; y < 8 && i < numData.Length; y++)
            {
                for (int x = 0; x < 8 && i < numData.Length; x++, i++)
                {
                    if (numData[i] != 0)
                    {
                        piecesData.Add((new((PieceType)Math.Abs(numData[i]), numData[i] > 0 ? Side.White : Side.Black), new(x, y)));
                    }
                }
            }

            SetPieces(piecesData);
        }

        private void SetPieces(IEnumerable<(Piece, Position)> piecesData)
        {
            Array.Clear(data);

            foreach (var (piece, pos) in piecesData)
            {
                this[pos] = piece;
            }
        }

        public int[] GetPiecesData()
        {
            return data.Select(b => (int)b).ToArray();
        }
    }
}
