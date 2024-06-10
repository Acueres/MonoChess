using System.Collections.Generic;
using System.Threading.Tasks;

using MonoChess.Models;
using MonoChess.Algorithms;
using MonoChess.Enums;

namespace MonoChess.Controllers
{
    class AIController : IController
    {
        readonly Board board;
        readonly Dictionary<AlgorithmType, IAlgorithm> algorithms;

        public AIController(Board board)
        {
            this.board = board;
            algorithms = new()
            {
                [AlgorithmType.AlphaBeta] = new AlphaBeta(),
                [AlgorithmType.Negamax] = new NegaMax(),
                [AlgorithmType.Random] = new Randomized()
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
