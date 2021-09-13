using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.Models;
using MonoChess.Algorithms;


namespace MonoChess.Controllers
{
    class AIController : IController
    {
        readonly Board board;
        IAlgorithm algorithm;

        public AIController(Board board)
        {
            this.board = board;
            algorithm = new Randomized();
        }

        public Move NextMove(Sides side, ChessState state)
        {
            return algorithm.CalculateMove(side, state, board);
        }
    }
}
