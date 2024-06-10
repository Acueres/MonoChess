using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using MonoChess.Models;
using MonoChess.Enums;

namespace MonoChess.Controllers
{
    class PlayerController : IController
    {
        public Piece SelectedPiece { get; set; } = Piece.Null;
        public List<Move> AllowedMoves { get; private set; } = new();
        public List<Move> DisallowedMoves { get; private set; } = new();
        public bool Interrupt() => stopTask = true;

        readonly Board board;
        const int tileSize = Board.SIZE / 8;
        bool stopTask;

        public PlayerController(Board board)
        {
            this.board = board;
        }

        public async Task<Move> NextMoveAsync(GameParameters parameters, Sides side, ChessState state)
        {
            Move move = Move.Null;
            stopTask = false;

            await Task.Run(async () =>
            {
                MouseState prevMs = Mouse.GetState();

                bool clicked;
                do
                {
                    MouseState ms = Mouse.GetState();
                    clicked = Util.MouseClicked(ms.LeftButton, prevMs.LeftButton);
                    prevMs = ms;

                    if (clicked)
                    {
                        move = NextMove(side);
                    }

                    //checks for input only every 10 milliseconds for performance issues
                    await Task.Delay(10);
                }
                while (move.IsNull && !stopTask);
            });

            return move;
        }

        public Move NextMove(Sides side)
        {
            MouseState ms = Mouse.GetState();

            var mouseVec = (new Vector2(ms.X, ms.Y) / tileSize);
            var mousePos = new Position(Math.Clamp((int)mouseVec.X, 0, 7), Math.Clamp((int)mouseVec.Y, 0, 7));

            Move move = Move.Null;

            if (!SelectedPiece.IsNull)
            {
                if (AllowedMoves.Any(m => m.TargetPosition == mousePos))
                {
                    move = new Move(SelectedPiece, mousePos);
                }

                if (!move.IsNull || mousePos == SelectedPiece.Position)
                {
                    SelectedPiece = Piece.Null;
                    return move;
                }
            }

            if (!board[mousePos].IsNull && board[mousePos].Side == side)
            {
                AllowedMoves.Clear();
                DisallowedMoves.Clear();

                SelectedPiece = board[mousePos];
                foreach (var m in board.GenerateMoves(SelectedPiece))
                {
                    if (board.DetectCheck(side, m))
                    {
                        DisallowedMoves.Add(m);
                    }
                    else
                    {
                        AllowedMoves.Add(m);
                    }
                }
            }

            return move;
        }
    }
}
