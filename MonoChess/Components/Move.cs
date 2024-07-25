using MonoChess.Enums;

namespace MonoChess.Components
{
    public readonly struct Move(Piece piece, Position current, Position target)
    {
        public Piece Piece => piece;
        public Position CurrentPosition => current;
        public Position TargetPosition => target;
        public readonly bool IsNull => Piece.IsNull;

        public static Move Null => new();

        public readonly bool CastlingCondition(Piece target)
        {
            return Piece.Side == target.Side && Piece.Type == PieceType.King && target.Type == PieceType.Rook;
        }

        public readonly bool PromotionCondition()
        {
            return Piece.Type == PieceType.Pawn
                && ((Piece.Side == Side.White && TargetPosition.Y == 0)
                || (Piece.Side == Side.Black && TargetPosition.Y == 7));
        }
    }
}
