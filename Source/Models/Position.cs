using System;

namespace MonoChess.Models
{
    public struct Position
    {
        public sbyte X { get; set; } //rank
        public sbyte Y { get; set; } //file

        public bool Orthogonal { get => Math.Abs(X) + Math.Abs(Y) == 1; }

        public Position(int x, int y)
        {
            X = (sbyte)x;
            Y = (sbyte)y;
        }

        public bool InBounds(int lower, int upper)
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

        public override string ToString()
        {
            return $"{{x-rank: {X}, y-file: {Y}}}";
        }

        public override int GetHashCode()
        {
            return Tuple.Create(X, Y).GetHashCode();
        }
    }
}
