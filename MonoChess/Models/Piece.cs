using System;
using System.Linq;
using System.Collections.Generic;

namespace MonoChess.Models
{
    public struct Piece
    {
        private readonly sbyte data = 0;

        public Pieces Type { get => byteToType[(byte)Math.Abs(data)]; }
        public Sides Side { get => (Sides)Math.Sign(data); }
        public Position Position { get; set; }
        public sbyte Data { get => data; }

        public bool CanCastle { get => Type == Pieces.Rook || Type == Pieces.King; }
        public bool IsNull { get => data == 0; }
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
            [Pieces.Null] = 0,
            [Pieces.Pawn] = 1,
            [Pieces.Knight] = 3,
            [Pieces.Bishop] = 3,
            [Pieces.Rook] = 5,
            [Pieces.Queen] = 9,
            [Pieces.King] = 1000
        };

        readonly static Dictionary<byte, Pieces> byteToType = new();

        readonly static Dictionary<int, string> names = new();

        public Piece(Pieces type, Sides side, Position position)
        {
            data = (sbyte)((int)side * (int)type);
            Position = position;
        }

        public Piece(sbyte data, Position position)
        {
            this.data = data;
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

            var pieces = Enum.GetValues(typeof(Pieces)).Cast<Pieces>().ToArray();

            foreach (var type in pieces)
            {
                if (type == Pieces.Null) continue;
                names.Add(HashCode.Combine(type, Sides.White), "w_" + type.ToString().ToLower());
                names.Add(HashCode.Combine(type, Sides.Black), "b_" + type.ToString().ToLower());
            }

            for (byte b = 0; b < pieces.Length; b++)
            {
                byteToType.Add(b, pieces[b]);
            }
        }

        public static Pieces GetType(sbyte value)
        {
            return byteToType[(byte)Math.Abs(value)];
        }

        public static Sides GetSide(sbyte value)
        {
            return (Sides)Math.Sign(value);
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
