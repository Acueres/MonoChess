using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MonoChess.Models;

namespace MonoChess.Algorithms
{
    interface IAlgorithm
    {
        public Move CalculateMove(Sides side, ChessState state, Board board);
    }
}
