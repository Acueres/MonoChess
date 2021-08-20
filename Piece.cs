namespace SharpChess
{
    public class Piece
    {
        public Pieces Type { get; set; }
        public Sides Side { get; set; }
        public Position Position { get; set; }

        public Piece(Pieces type, Sides side, Position position)
        {
            Type = type;
            Side = side;
            Position = position;
        }
    }
}
