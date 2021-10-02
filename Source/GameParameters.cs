using MonoChess.Algorithms;

namespace MonoChess
{
    public class GameParameters
    {
        public Sides PlayerSide { get; set; }
        public bool SinglePlayer { get; set; }
        public bool ShowGrid { get; set; }
        public Algorithm AlgorithmType { get; set; }
        public int Depth { get; set; } = 3;
    }
}
