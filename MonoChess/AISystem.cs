using MonoChess.Algorithms;
using MonoChess.Components;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoChess
{
    class AISystem
    {
        readonly Board board;
        readonly Dictionary<AlgorithmType, IAlgorithm> algorithms;

        public AISystem(Board board)
        {
            this.board = board;
            algorithms = new()
            {
                [AlgorithmType.AlphaBeta] = new AlphaBeta(),
                [AlgorithmType.Negamax] = new NegaMax(),
                [AlgorithmType.Random] = new Randomized()
            };
        }

        public async Task<Move> NextMoveAsync(GameParameters parameters, Side side, ChessState state)
        {
            Move move = Move.Null;

            await Task.Run(() => move = NextMove(parameters, side, state));

            return move;
        }

        public Move NextMove(GameParameters parameters, Side side, ChessState state)
        {
            return algorithms[parameters.AlgorithmType].CalculateMove(parameters.Depth, side, state, board);
        }
    }
}
