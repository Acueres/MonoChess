using System;
using System.Collections.Generic;
using System.Linq;

using MonoChess.Models;

namespace MonoChess
{
    public class Board
    {
        public const int SIZE = 504;

        public bool[] CastlingData 
        { 
            get => castling;
            set => Array.Copy(value, castling, value.Length < 2 ? value.Length : 2);
        }
        readonly bool[] castling = { true, true };

        Dictionary<Position, Piece> pieces = new();

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
            pieces = new(board.pieces);
            Array.Copy(castling, board.castling, 2);
        }

        public Piece this[Position pos]
        {
            get => pieces.ContainsKey(pos) ? pieces[pos] : Piece.Null;
            set
            {
                if (value.Type == Pieces.Null)
                {
                    pieces.Remove(pos);
                }
                else if (pieces.ContainsKey(pos))
                {
                    pieces[pos] = value;
                }
                else
                {
                    pieces.Add(pos, value);
                }
            }
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
                if (castling[(int)move.Piece.Side] && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece))
                {
                    castling[(int)move.Piece.Side] = false;
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

            castling[(int)side] = false;

            return rook;
        }

        public void ReverseMove(Move move, Piece removedPiece)
        {
            if (move.CastlingCondition(removedPiece))
            {
                ReverseCastlingMove(move.TargetPosition, move.Piece.Side);
                return;
            }
            else if (!castling[(int)move.Piece.Side] && move.Piece.CanCastle && CastlingCandidateMovement(move.Piece))
            {
                castling[(int)move.Piece.Side] = true;
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

            pieces.Remove(oldKingPos);
            pieces.Remove(oldRookPos);

            pieces.Add(kingPos, new Piece(Pieces.King, side, kingPos));
            pieces.Add(rookPos, new Piece(Pieces.Rook, side, rookPos));

            castling[(int)side] = true;
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

            var rooks = pieces.Values.Where(p => p.Side == side && p.Type == Pieces.Rook).ToArray();

            if (rooks.Length == 1)
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

                var targetPos = piece.Position - direction;

                while (targetPos.InBounds(0, 8))
                {
                    if (pieces.ContainsKey(targetPos))
                    {
                        if (piece.Type == Pieces.Pawn && direction.Orthogonal)
                        {
                            break; //prevent pawn straight attack
                        }

                        if (pieces[targetPos].Side != piece.Side)
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
                        if (piece.Type == Pieces.Pawn && !pieces.ContainsKey(targetPos - direction) &&
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

            if (piece.Type == Pieces.King && castling[(int)piece.Side])
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
            return pieces.Values.ToArray().SingleOrDefault(p => p.Type == Pieces.King && p.Side == side);
        }

        public void SetPieces()
        {
            pieces.Clear();
            castling[0] = true;
            castling[1] = true;

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
            pieces.Clear();

            foreach (var piece in piecesData)
            {
                pieces.Add(piece.Position, piece);
            }

            pieces = pieces.OrderBy(x => new Random().Next()).ToDictionary(item => item.Key, item => item.Value);
        }

        public int[] GetPiecesData()
        {
            int[] data = new int[8 * 8];

            int i = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var pos = new Position(x, y);
                    if (pieces.ContainsKey(pos))
                    {
                        //pieces coded by their enum value, white positive, black negative
                        data[i] = (int)pieces[pos].Type * (pieces[pos].Side == Sides.White ? 1 : -1);
                    }

                    i++;
                }
            }

            return data;
        }
    }
}
