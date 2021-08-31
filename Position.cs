﻿using System;

namespace MonoChess
{
    public struct Position
    {
        public int X { get; set; } //rank
        public int Y { get; set; } //file

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position Direction()
        {
            return new Position(Math.Sign(X), Math.Sign(Y));
        }

        public static Position operator +(Position val1, Position val2)
        {
            return new Position(val1.X + val2.X, val1.Y + val2.Y);
        }

        public static Position operator -(Position val1, Position val2)
        {
            return new Position(val1.X - val2.X, val1.Y - val2.Y);
        }

        public static Position operator *(Position val, int multiplier)
        {
            return new Position(val.X * multiplier, val.Y * multiplier);
        }

        public static Position operator / (Position val, int divider)
        {
            val.X /= divider;
            val.Y /= divider;

            return val;
        }

        public static bool operator ==(Position val1, Position val2)
        {
            return val1.X == val2.X && val1.Y == val2.Y;
        }

        public static bool operator !=(Position val1, Position val2)
        {
            return val1.X != val2.X || val1.Y != val2.Y;
        }

        public override string ToString()
        {
            return $"{{x-rank: {X}, y-file: {Y}}}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            Position p = (Position)obj;
            return X == p.X && Y == p.Y;

        }

        public override int GetHashCode()
        {
            return Tuple.Create(X, Y).GetHashCode();
        }
    }
}
