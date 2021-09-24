namespace MonoChess.Models
{
    public struct Move
    {
        public Piece Piece { get; set; }
        public Position Position { get; set; }
        public bool IsNull { get => Piece.IsNull;  }
        public static Move Null { get => new(); }

        public Move(Piece piece, Position position)
        {
            Piece = piece;
            Position = position;
        }
    }
}
