using MonoChess.Components;

namespace MonoChess.Algorithms
{
    public class NegaMax : IAlgorithm
    {
        Board board;

        public Move CalculateMove(int depth, Side side, ChessState state, Board board)
        {
            if (state == ChessState.Opening)
            {
                return new Move(board[new(4, 6)], new(4, 6), new(4, 4));
            }

            bool check = (state == ChessState.WhiteCheck && side == Side.White)
                || (state == ChessState.BlackCheck && side == Side.Black);

            Move bestMove = Move.Null;
            this.board = new(board);

            int max = -int.MaxValue;

            foreach (var move in board.GenerateMoves(side))
            {
                if (check && board.DetectCheck(side, move))
                {
                    continue;
                }

                this.board.MakeMove(move, out var removed);
                int score = -CalculateScore(depth - 1, side);
                this.board.ReverseMove(move, removed);
               
                if (score > max)
                {
                    max = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        int CalculateScore(int depth, Side side)
        {
            side = Util.ReverseSide(side);

            if (depth == 0) 
            {
                return board.GetScore(side);
            }

            int max = -int.MaxValue;
            foreach (var move in board.GenerateMoves(side))
            {
                board.MakeMove(move, out var removed);
                int score = -CalculateScore(depth - 1, side);
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
