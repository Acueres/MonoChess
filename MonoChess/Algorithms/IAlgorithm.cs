using MonoChess.Models;
using MonoChess.Enums;

namespace MonoChess.Algorithms
{
    interface IAlgorithm
    {
        public Move CalculateMove(int depth, Sides side, ChessState state, Board board);
    }
}
