using System;
using System.Linq;
using System.Collections.Generic;

using MonoChess.Enums;

namespace MonoChess.Models
{
    public struct Piece
    {
        private readonly sbyte data = 0;

        public readonly PieceType Type => byteToType[(byte)Math.Abs(data)];
        public readonly Side Side => (Side)Math.Sign(data);
        public Position Position { get; set; }
        public readonly sbyte Data => data;

        public readonly bool CanCastle => Type == PieceType.Rook || Type == PieceType.King;
        public readonly bool IsNull => data == 0;
        public readonly Position[] Directions => directions[Type];
        public readonly bool RangeLimited => rangeLimited[Type];
        public readonly int Score => scores[Type];

        public Piece(PieceType type, Side side, Position position)
        {
            data = (sbyte)((int)side * (int)type);
            Position = position;
        }

        public Piece(sbyte data, Position position)
        {
            this.data = data;
            Position = position;
        }

        public static Piece Null => new();

        readonly static Dictionary<PieceType, Position[]> directions = [];
        readonly static Dictionary<PieceType, bool> rangeLimited = new()
        {
            [PieceType.Pawn] = true,
            [PieceType.Knight] = true,
            [PieceType.King] = true,
            [PieceType.Bishop] = false,
            [PieceType.Rook] = false,
            [PieceType.Queen] = false
        };

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

        readonly static Dictionary<byte, PieceType> byteToType = [];
        readonly static Dictionary<string, PieceType> nameToPieceType = [];

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

            directions.Add(PieceType.Pawn, [new(0, 1), new(1, 1), new(-1, 1)]);
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

            for (byte b = 0; b < pieces.Length; b++)
            {
                byteToType.Add(b, pieces[b]);
            }
        }

        public static PieceType GetType(sbyte value)
        {
            return byteToType[(byte)Math.Abs(value)];
        }

        public static Side GetSide(sbyte value)
        {
            return (Side)Math.Sign(value);
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
