namespace MonoChess.Models
{
    public struct Move
    {
        public Piece Piece { get; set; }
        public Position TargetPosition { get; set; }
        public bool IsNull { get => Piece.IsNull;  }

        public static Move Null { get; } = new();

        public Move(Piece piece, Position target)
        {
            Piece = piece;
            TargetPosition = target;
        }

        public bool CastlingCondition(Piece target)
        {
            return Piece.Side == target.Side && Piece.Type == Pieces.King && target.Type == Pieces.Rook;
        }

        public bool PromotionCondition()
        {
            return Piece.Type == Pieces.Pawn
                && ((Piece.Side == Sides.White && TargetPosition.Y == 0)
                || (Piece.Side == Sides.Black && TargetPosition.Y == 7));
        }
    }
}
