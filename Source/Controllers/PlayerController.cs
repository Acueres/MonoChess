using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;

using MonoChess.Models;

namespace MonoChess.Controllers
{
    class PlayerController : IController
    {
        public Piece DraggedPiece { get; set; }
        public List<Move> AllowedMoves { get; private set; } = new();

        readonly Board board;
        readonly int tileSize;

        public PlayerController(Board board)
        {
            this.board = board;
            tileSize = GameParameters.BOARD_WIDTH / 8;
        }

        public Move NextMove(Sides side, ChessState state)
        {
            MouseState ms = Mouse.GetState();

            if (ms.Y < GameParameters.MENU_HEIGHT) return Move.Null; //prevent black piece selection when clicking on the menu

            var pos = (new Position(ms.X, ms.Y - GameParameters.MENU_HEIGHT) / tileSize);

            pos.X = Math.Clamp(pos.X, 0, 7);
            pos.Y = Math.Clamp(pos.Y, 0, 7);

            Move move = new();

            if (ms.LeftButton == ButtonState.Pressed && DraggedPiece.IsNull && !board[pos].IsNull && board[pos].Side == side)
            {
                DraggedPiece = board[pos]; //select a piece to move
                foreach (var m in board.GenerateMoves(DraggedPiece))
                {
                    if (!board.DetectCheck(side, m))
                    {
                        AllowedMoves.Add(m);
                    }
                }
            }

            if (!DraggedPiece.IsNull && ms.LeftButton == ButtonState.Released)
            {
                if ((board[pos].IsNull || board[pos].Side != DraggedPiece.Side) && AllowedMoves.Any(m => m.Position == pos))
                {
                    move = new Move(DraggedPiece, pos);
                }

                DraggedPiece = new Piece();
                AllowedMoves.Clear();
            }

            return move;
        }
    }
}
