using MonoChess.Components;

namespace MonoChess.Algorithms
{
    public enum AlgorithmType : byte
    {
        AlphaBeta,
        Negamax,
        Random
    }

    interface IAlgorithm
    {
        public Move CalculateMove(int depth, Side side, ChessState state, Board board);
    }
}
