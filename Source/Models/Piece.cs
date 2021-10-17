using System;
using System.Linq;
using System.Collections.Generic;

namespace MonoChess.Models
{
    public struct Piece
    {
        public Pieces Type { get; set; }
        public Sides Side { get; set; }
        public Position Position { get; set; }

        public bool CanCastle { get => Type == Pieces.Rook || Type == Pieces.King; }
        public bool IsNull { get => Type == Pieces.Null; }
        public Position[] Directions { get => directions[Type]; }
        public bool RangeLimited { get => rangeLimited[Type]; }
        public int Score { get => scores[Type]; }
        public string Name { get => names[HashCode.Combine(Type, Side)]; }
        public static Piece Null { get; } = new();

        readonly static Dictionary<Pieces, Position[]> directions = new();
        readonly static Dictionary<Pieces, bool> rangeLimited = new()
        {
            [Pieces.Pawn] = true,
            [Pieces.Knight] = true,
            [Pieces.King] = true,
            [Pieces.Bishop] = false,
            [Pieces.Rook] = false,
            [Pieces.Queen] = false
        };
        readonly static Dictionary<Pieces, int> scores = new()
        {
            [Pieces.Pawn] = 1,
            [Pieces.Knight] = 3,
            [Pieces.Bishop] = 3,
            [Pieces.Rook] = 5,
            [Pieces.Queen] = 9,
            [Pieces.King] = 1000
        };
        readonly static Dictionary<int, string> names = new();

        public Piece(Pieces type, Sides side, Position position)
        {
            Type = type;
            Side = side;
            Position = position;
        }

        static Piece()
        {
            var orthogonal = new Position[] { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
            var diagonal = new Position[] { new(1, 1), new(-1, -1), new(1, 1), new(-1, 1), new(1, -1) };
            var omnidirectional = orthogonal.Concat(diagonal).ToArray();
            var knightMoves = new Position[]
            {
                new(-1, 2), new(1, 2),
                new(2, -1), new(2, 1),
                new(-1, -2), new(1, -2),
                new(-2, -1), new(-2, 1)
            };

            directions.Add(Pieces.Pawn, new Position[] { new(0, 1), new(1, 1), new(-1, 1) });
            directions.Add(Pieces.King, omnidirectional);
            directions.Add(Pieces.Queen, omnidirectional);
            directions.Add(Pieces.Rook, orthogonal);
            directions.Add(Pieces.Bishop, diagonal);
            directions.Add(Pieces.Knight, knightMoves);

            foreach (var side in new Sides[] {Sides.White, Sides.Black })
            {
                foreach (var type in Enum.GetValues(typeof(Pieces)).Cast<Pieces>())
                {
                    if (type == Pieces.Null) continue;
                    names.Add(HashCode.Combine(type, side), (side == Sides.White ? "w" : "b") + "_" + type.ToString().ToLower());
                }
            }
        }

        public static bool operator ==(Piece p1, Piece p2)
        {
            return p1.Type == p2.Type && p1.Side == p2.Side && p1.Position == p2.Position;
        }

        public static bool operator !=(Piece p1, Piece p2)
        {
            return p1.Type != p2.Type || p1.Side != p2.Side || p1.Position != p2.Position;
        }

        public override string ToString()
        {
            return $"{Type}, {Side}, {Position}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Side, Position);
        }
    }
}
