namespace MonoChess
{
    public class GameParameters
    {
        public const int BOARD_WIDTH = 504;
        public const int MENU_HEIGHT = 80;

        public Sides Side { get; set; }
        public bool SinglePlayer { get; set; }
    }
}
