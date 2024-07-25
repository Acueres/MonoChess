using System;

namespace MonoChess.Components
{
    public readonly struct Position
    {
        public readonly sbyte X { get; }
        public readonly sbyte Y { get; }

        public Position(int x, int y)
        {
            X = (sbyte)x;
            Y = (sbyte)y;
        }

        public Position()
        {
            X = -1;
            Y = -1;
        }

        public readonly bool IsNull => X == -1;

        public readonly bool Orthogonal => Math.Abs(X) + Math.Abs(Y) == 1;

        public readonly bool InBounds(int lower, int upper)
        {
            return X >= lower && X < upper && Y >= lower && Y < upper;
        }

        public static Position Null => new();

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
