using MonoChess.Enums;

namespace MonoChess.Models
{
    public struct Move(Piece piece, Position target)
    {
        public Piece Piece { get; set; } = piece;
        public Position TargetPosition { get; set; } = target;
        public readonly bool IsNull => Piece.IsNull;

        public static Move Null { get; } = new();

        public readonly bool CastlingCondition(Piece target)
        {
            return Piece.Side == target.Side && Piece.Type == Pieces.King && target.Type == Pieces.Rook;
        }

        public readonly bool PromotionCondition()
        {
            return Piece.Type == Pieces.Pawn
                && ((Piece.Side == Sides.White && TargetPosition.Y == 0)
                || (Piece.Side == Sides.Black && TargetPosition.Y == 7));
        }
    }
}
