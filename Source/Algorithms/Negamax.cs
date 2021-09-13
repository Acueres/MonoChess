using MonoChess.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MonoChess.Algorithms
{
    public class Negamax : IAlgorithm
    {
        Board board;

        public Move CalculateMove(Sides side, ChessState state, Board board)
        {
            if (state == ChessState.Opening)
            {
                return new Move(board[new(4, 6)], new(4, 4));
            }

            Move bestMove = new();
            this.board = new(board);
            int max = -int.MaxValue;

            foreach (var move in board.GenerateMoves(side))
            {
                this.board.MakeMove(move, out var removed);
                int score = -NegaMax(2, side);
                this.board.ReverseMove(move, removed);
                if (score > max)
                {
                    max = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        int NegaMax(int depth, Sides side)
        {
            side = side == Sides.White ? Sides.Black : Sides.White;

            if (depth == 0) 
            {
                return board.GetScore(side);
            }

            int max = -int.MaxValue;
            foreach (var move in board.GenerateMoves(side))
            {
                board.MakeMove(move, out var removed);
                int score = -NegaMax(depth - 1, side);
                board.ReverseMove(move, removed);
                if (score > max)
                {
                    max = score;
                }
            }
            return max;
        }
    }
}
