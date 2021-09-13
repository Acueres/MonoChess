﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.Models;

namespace MonoChess.Algorithms
{
    class Randomized : IAlgorithm
    {
        readonly Random rnd = new();
        readonly List<Move> possibleMoves = new(1000);

        public Move CalculateMove(Sides side, ChessState state, Board board)
        {
            possibleMoves.Clear();

            if (state == ChessState.Opening)
            {
                return new Move(board[new(4, 6)], new(4, 4));
            }

            foreach (var move in board.GenerateMoves(side))
            {
                possibleMoves.Add(move);
            }

            if (possibleMoves.Count == 0) return new Move();

            int index = rnd.Next(0, possibleMoves.Count - 1);
            return possibleMoves[index];
        }
    }
}
