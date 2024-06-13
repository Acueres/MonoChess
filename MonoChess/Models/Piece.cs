using System;
using System.Linq;
using System.Collections.Generic;

using MonoChess.Enums;

namespace MonoChess.Models
{
    public struct Piece
    {
        private readonly sbyte data = 0;

        public readonly Pieces Type => byteToType[(byte)Math.Abs(data)];
        public readonly Sides Side => (Sides)Math.Sign(data);
        public Position Position { get; set; }
        public readonly sbyte Data => data;

        public readonly bool CanCastle => Type == Pieces.Rook || Type == Pieces.King;
        public readonly bool IsNull => data == 0;
        public readonly Position[] Directions => directions[Type];
        public readonly bool RangeLimited => rangeLimited[Type];
        public readonly int Score => scores[Type];
        public readonly string Name => names[HashCode.Combine(Type, Side)];
        public static Piece Null => new();

        readonly static Dictionary<Pieces, Position[]> directions = [];

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

        readonly static Dictionary<byte, Pieces> byteToType = [];

        readonly static Dictionary<int, string> names = [];

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

            directions.Add(Pieces.Pawn, [new(0, 1), new(1, 1), new(-1, 1)]);
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

        public readonly override string ToString()
        {
            return $"{Type}, {Side}, {Position}";
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Type, Side, Position);
        }

        public readonly override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Piece piece)
            {
                return this == piece;
            }

            return false;
        }
    }
}
