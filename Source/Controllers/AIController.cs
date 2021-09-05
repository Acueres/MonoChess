using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.Models;


namespace MonoChess.Controllers
{
    class AIController
    {
        readonly Board board;
        readonly Random rnd = new();
        readonly List<Move> possibleMoves = new(1000);

        public AIController(Board board)
        {
            this.board = board;
        }

        public Move NextMove(Sides side)
        {
            possibleMoves.Clear();

            foreach (var piece in board.GetPieces(side))
            {
                foreach (var move in board.GenerateMoves(piece))
                {
                    possibleMoves.Add(move);
                }
            }

            int index = rnd.Next(0, possibleMoves.Count() - 1);
            return possibleMoves[index];
        }
    }
}
