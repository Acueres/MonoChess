using System;

namespace MonoChess
{
    public struct Position
    {
        public int Rank { get; set; } //x-axis
        public int File { get; set; } //y-axis

        public Position(int rank, int file)
        {
            Rank = rank;
            File = file;
        }

        public Position(int[] pos)
        {
            Rank = pos[0];
            File = pos[1];
        }

        public static Position operator / (Position val, int divider)
        {
            val.Rank /= divider;
            val.File /= divider;

            return val;
        }

        public static bool operator ==(Position val1, Position val2)
        {
            return val1.Rank == val2.Rank && val1.File == val2.File;
        }

        public static bool operator !=(Position val1, Position val2)
        {
            return val1.Rank != val2.Rank || val1.File != val2.File;
        }

        public override string ToString()
        {
            return $"{{rank: {Rank}, file: {File}}}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            Position p = (Position)obj;
            return Rank == p.Rank && File == p.File;

        }

        public override int GetHashCode()
        {
            return Tuple.Create(Rank, File).GetHashCode();
        }
    }
}
