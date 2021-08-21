namespace MonoChess
{
    public class Piece
    {
        public Pieces Type { get; set; }
        public Sides Side { get; set; }
        public Position Position { get; set; }
        public string Name { get; set; }

        public Piece(Pieces type, Sides side, Position position)
        {
            Type = type;
            Side = side;
            Position = position;
            Name = (Side == Sides.White ? "w" : "b") + "_" + Type.ToString().ToLower();
        }

        public Piece Clone()
        {
            return new Piece(Type, Side, Position);
        }

        public override string ToString()
        {
            return $"{Type}, {Side}, {Position}";
        }
    }
}
