namespace SharpChess
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

        public override string ToString()
        {
            return $"{{rank: {Rank}, file: {File}}}";
        }
    }
}
