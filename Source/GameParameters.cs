namespace MonoChess
{
    public class GameParameters
    {
        public const int BOARD_WIDTH = 504;

        public Sides Side { get; set; }
        public bool SinglePlayer { get; set; }
        public bool ShowGrid { get; set; }
    }
}
