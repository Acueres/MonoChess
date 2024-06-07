using System;

using MonoChess.Models;

namespace MonoChess.Algorithms
{
    public enum Algorithm
    {
        AlphaBeta,
        NegaMax,
        Randomized,
        Count
    }

    interface IAlgorithm
    {
        public Move CalculateMove(int depth, Sides side, ChessState state, Board board);
    }
}
