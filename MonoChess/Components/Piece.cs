using System;
using System.Linq;
using System.Collections.Generic;

using MonoChess.Enums;

namespace MonoChess.Components
{
    public readonly struct Piece
    {
        private readonly sbyte value = 0;

        public readonly PieceType Type => (PieceType)Math.Abs(value);
        public readonly Side Side => (Side)Math.Sign(value);
        public readonly sbyte Value => value;

        public readonly bool CanCastle => Type == PieceType.Rook || Type == PieceType.King;
        public readonly bool IsNull => value == 0;
        public readonly Position[] Directions => directions[Type];
        public readonly bool RangeLimited => rangeLimited.Contains(Type);
        public readonly int Score => scores[Type];

        public Piece(PieceType type, Side side)
        {
            value = (sbyte)(Math.Sign((int)side) == 1 ? (int)type : -(int)type);
        }

        public Piece(sbyte data)
        {
            value = data;
        }

        public static Piece Null => new();

        readonly static Dictionary<PieceType, Position[]> directions = [];
        readonly static HashSet<PieceType> rangeLimited =
        [
            PieceType.Pawn,
            PieceType.Knight,
            PieceType.King,
        ];

        readonly static Dictionary<PieceType, int> scores = new()
        {
            [PieceType.Null] = 0,
            [PieceType.Pawn] = 1,
            [PieceType.Knight] = 3,
            [PieceType.Bishop] = 3,
            [PieceType.Rook] = 5,
            [PieceType.Queen] = 9,
            [PieceType.King] = 1000
        };

        readonly static Dictionary<string, PieceType> nameToPieceType = [];

        static Piece()
        {
            Position[] orthogonal = [ new(0, 1), new(0, -1), new(1, 0), new(-1, 0) ];
            Position[] diagonal = [ new(1, 1), new(-1, -1), new(1, 1), new(-1, 1), new(1, -1) ];
            var omnidirectional = orthogonal.Concat(diagonal).ToArray();
            Position[] knightMoves =
            [
                new(-1, 2), new(1, 2),
                new(2, -1), new(2, 1),
                new(-1, -2), new(1, -2),
                new(-2, -1), new(-2, 1)
            ];

            directions.Add(PieceType.Pawn, [ new(0, 1), new(1, 1), new(-1, 1)] );
            directions.Add(PieceType.King, omnidirectional);
            directions.Add(PieceType.Queen, omnidirectional);
            directions.Add(PieceType.Rook, orthogonal);
            directions.Add(PieceType.Bishop, diagonal);
            directions.Add(PieceType.Knight, knightMoves);

            var pieces = Enum.GetValues(typeof(PieceType)).Cast<PieceType>().ToArray();

            foreach (var type in pieces)
            {
                if (type == PieceType.Null) continue;
                nameToPieceType.Add(type.ToString().ToLower(), type);
            }
        }

        public static bool operator ==(Piece p1, Piece p2)
        {
            return p1.Type == p2.Type && p1.Side == p2.Side;
        }

        public static bool operator !=(Piece p1, Piece p2)
        {
            return p1.Type != p2.Type || p1.Side != p2.Side;
        }

        public readonly override string ToString()
        {
            return $"{Type}, {Side}";
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Type, Side);
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
