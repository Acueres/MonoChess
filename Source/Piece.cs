﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace MonoChess
{
    public struct Piece
    {
        public Pieces Type { get; set; }
        public Sides Side { get; set; }
        public Position Position { get; set; }
        public string Name { get; set; }
        public bool IsNull
        {
            get
            {
                return Type == Pieces.Null;
            }
        }

        public static Dictionary<Pieces, Position[]> Directions { get; set; } = new();
        public static Dictionary<Pieces, bool> RangeLimited { get; set; } = new()
        {
            [Pieces.Pawn] = true,
            [Pieces.Knight] = true,
            [Pieces.King] = true,
            [Pieces.Bishop] = false,
            [Pieces.Rook] = false,
            [Pieces.Queen] = false
        };

        public Piece(Pieces type, Sides side, Position position)
        {
            Type = type;
            Side = side;
            Position = position;
            Name = (Side == Sides.White ? "w" : "b") + "_" + Type.ToString().ToLower();
        }

        static Piece()
        {
            var principal = new Position[] { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
            var diagonal = new Position[] { new(1, 1), new(-1, -1), new(1, 1), new(-1, 1), new(1, -1) };
            var omnidirectional = principal.Concat(diagonal).ToArray();
            var knightMoves = new Position[]
            {
                new(-1, 2), new(1, 2),
                new(2, -1), new(2, 1),
                new(-1, -2), new(1, -2),
                new(-2, -1), new(-2, 1)
            };

            Directions.Add(Pieces.Pawn, new Position[] { new(0, 1), new(1, 1), new(-1, 1) });
            Directions.Add(Pieces.King, omnidirectional);
            Directions.Add(Pieces.Queen, omnidirectional);
            Directions.Add(Pieces.Rook, principal);
            Directions.Add(Pieces.Bishop, diagonal);
            Directions.Add(Pieces.Knight, knightMoves);
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
    }
}
