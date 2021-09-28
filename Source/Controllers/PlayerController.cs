using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;

using MonoChess.Models;

namespace MonoChess.Controllers
{
    class PlayerController : IController
    {
        public Piece SelectedPiece { get; set; } = Piece.Null;
        public List<Move> AllowedMoves { get; private set; } = new();
        public List<Move> DisallowedMoves { get; private set; } = new();

        readonly Board board;
        readonly int tileSize;

        MouseState prevMs = Mouse.GetState();


        public PlayerController(Board board)
        {
            this.board = board;
            tileSize = GameParameters.BOARD_WIDTH / 8;
        }

        public Move NextMove(Sides side, ChessState state)
        {
            MouseState ms = Mouse.GetState();

            var mousePos = (new Position(ms.X, ms.Y) / tileSize);

            mousePos.X = Math.Clamp(mousePos.X, 0, 7);
            mousePos.Y = Math.Clamp(mousePos.Y, 0, 7);

            Move move = Move.Null;

            if (!SelectedPiece.IsNull && Util.MouseClicked(ms.LeftButton, prevMs.LeftButton))
            {
                if (AllowedMoves.Any(m => m.Position == mousePos))
                {
                    move = new Move(SelectedPiece, mousePos);
                }

                if (!move.IsNull || mousePos == SelectedPiece.Position)
                {
                    SelectedPiece = Piece.Null;
                    prevMs = ms;
                    return move;
                }
            }

            if (Util.MouseClicked(ms.LeftButton, prevMs.LeftButton)
                && !board[mousePos].IsNull && board[mousePos].Side == side)
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

            prevMs = ms;
            return move;
        }
    }
}
