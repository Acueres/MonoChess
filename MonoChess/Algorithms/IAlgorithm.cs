using MonoChess.Components;
using MonoChess.Enums;

namespace MonoChess.Algorithms
{
    interface IAlgorithm
    {
        public Move CalculateMove(int depth, Side side, ChessState state, Board board);
    }
}
