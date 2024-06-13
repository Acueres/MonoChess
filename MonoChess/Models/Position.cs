using System;

namespace MonoChess.Models
{
    public struct Position(int x, int y)
    {
        public sbyte X { get; set; } = (sbyte)x;
        public sbyte Y { get; set; } = (sbyte)y;

        public readonly bool Orthogonal => Math.Abs(X) + Math.Abs(Y) == 1;

        public readonly bool InBounds(int lower, int upper)
        {
            return X >= lower && X < upper && Y >= lower && Y < upper;
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

        public static Position operator / (Position val, sbyte divisor)
        {
            val.X /= divisor;
            val.Y /= divisor;

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

        public readonly override string ToString()
        {
            return $"{{x-rank: {X}, y-file: {Y}}}";
        }

        public readonly override int GetHashCode()
        {
            return Tuple.Create(X, Y).GetHashCode();
        }

        public readonly override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Position pos)
            {
                return this == pos;
            }

            return false;
        }
    }
}
