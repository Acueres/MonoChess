using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

using MonoChess.Models;

namespace MonoChess.Controllers
{
    class PlayerController : IController
    {
        public Piece DraggedPiece { get; set; }
        public List<Position> AllowedMoves { get; private set; } = new();

        readonly Board board;

        public PlayerController(Board board)
        {
            this.board = board;
        }

        public Move NextMove(Sides side)
        {
            MouseState ms = Mouse.GetState();
            var pos = GetCursorPosition(ms.X, ms.Y, Chess.ScreenWidth);
            pos.X = Math.Clamp(pos.X, 0, 7);
            pos.Y = Math.Clamp(pos.Y, 0, 7);

            Move move = new();

            if (ms.LeftButton == ButtonState.Pressed && DraggedPiece.IsNull && !board[pos].IsNull && board[pos].Side == side)
            {
                DraggedPiece = board[pos]; //select a piece to move
                foreach (var m in board.GenerateMoves(DraggedPiece))
                {
                    AllowedMoves.Add(m.Position);
                }
            }

            if (!DraggedPiece.IsNull && ms.LeftButton == ButtonState.Released)
            {
                if ((board[pos].IsNull || board[pos].Side != DraggedPiece.Side) && AllowedMoves.Contains(pos))
                {
                    move = new Move(DraggedPiece, pos);
                }

                DraggedPiece = new Piece();
                AllowedMoves.Clear();
            }

            return move;
        }

        //Gets cursor's file and rank position
        static Position GetCursorPosition(int x, int y, int size)
        {
            var pos = new Position(x, y);
            return pos / size;
        }
    }
}
