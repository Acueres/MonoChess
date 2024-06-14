using System.Threading.Tasks;

using MonoChess.Models;
using MonoChess.Enums;

namespace MonoChess.Controllers
{
    interface IController
    {
        public Task<Move> NextMoveAsync(GameParameters parameters, Side side, ChessState state);
    }
}
