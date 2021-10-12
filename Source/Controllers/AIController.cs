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
        readonly Dictionary<Algorithm, IAlgorithm> algorithms;

        public AIController(Board board)
        {
            this.board = board;
            algorithms = new()
            {
                [Algorithm.AlphaBeta] = new AlphaBeta(),
                [Algorithm.NegaMax] = new NegaMax(),
                [Algorithm.Randomized] = new Randomized()
            };
        }

        public async Task<Move> NextMoveAsync(GameParameters parameters, Sides side, ChessState state)
        {
            Move move = Move.Null;

            await Task.Run(() => move = NextMove(parameters, side, state));

            return move;
        }

        public Move NextMove(GameParameters parameters, Sides side, ChessState state)
        {
            return algorithms[parameters.AlgorithmType].CalculateMove(parameters.Depth, side, state, board);
        }
    }
}
