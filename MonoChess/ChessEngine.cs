using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

using MonoChess.Enums;
using MonoChess.Models;
using Microsoft.Xna.Framework.Input;

namespace MonoChess
{
    public class ChessEngine
    {
        readonly AssetServer assetServer;
        readonly SpriteBatch spriteBatch;

        readonly GameParameters parameters;
        readonly AISystem ai;
        readonly Board board = new();

        Task<Move> nextMoveTask;
        Side currentSide = Side.White;
        ChessState state;
        Move move = Move.Null;
        MouseState prevMs;
        Piece selectedPiece = Piece.Null;
        readonly List<Move> allowedMoves = [];
        readonly List<Move> disallowedMoves = [];

        bool PlayerTurn  => currentSide == parameters.PlayerSide || !parameters.SinglePlayer;
        double calculationTime;

        public ChessEngine(SpriteBatch spriteBatch, GameParameters parameters, AssetServer assetServer)
        {
            this.spriteBatch = spriteBatch;
            this.parameters = parameters;
            this.assetServer = assetServer;

            ai = new AISystem(board);

            prevMs = Mouse.GetState();
        }

        public void Reset()
        {
            nextMoveTask.Wait();

            currentSide = Side.White;
            selectedPiece = Piece.Null;
            move = Move.Null;
            board.SetPieces();
            state = ChessState.Opening;
        }

        public async Task Update()
        {
            if (parameters.GameState == GameState.End) return;

            if (board.DetectCheckmate(currentSide))
            {
                parameters.GameState = GameState.End;
                return;
            }

            if (board.DetectCheck(currentSide))
            {
                state = currentSide == Side.White ? ChessState.WhiteCheck : ChessState.BlackCheck;
            }
            else
            {
                state = ChessState.Default;
            }

            MouseState ms = Mouse.GetState();

            nextMoveTask = GetNextMove(ms);
            Move move = await nextMoveTask;

            prevMs = ms;

            if (move.IsNull) return;

            board.MakeMove(move, out _);

            currentSide = Util.ReverseSide(currentSide);
        }

        async Task<Move> GetNextMove(MouseState ms)
        {
            if (PlayerTurn)
            {
                bool clicked = Util.MouseClicked(ms.LeftButton, prevMs.LeftButton);

                if (clicked)
                {
                    return GetPlayerMove();
                }
            }
            else
            {
                calculationTime = 0;
                return await ai.NextMoveAsync(parameters, currentSide, state);
            }

            return Move.Null;
        }

        Move GetPlayerMove()
        {
            const int tileSize = Board.SIZE / 8;

            MouseState ms = Mouse.GetState();

            var mouseVec = (new Vector2(ms.X, ms.Y) / tileSize);
            var mousePos = new Position(Math.Clamp((int)mouseVec.X, 0, 7), Math.Clamp((int)mouseVec.Y, 0, 7));

            Move move = Move.Null;

            if (!selectedPiece.IsNull)
            {
                if (allowedMoves.Any(m => m.TargetPosition == mousePos))
                {
                    move = new Move(selectedPiece, mousePos);
                }

                if (!move.IsNull || mousePos == selectedPiece.Position)
                {
                    selectedPiece = Piece.Null;
                    return move;
                }
            }

            if (!board[mousePos].IsNull && board[mousePos].Side == currentSide)
            {
                allowedMoves.Clear();
                disallowedMoves.Clear();

                selectedPiece = board[mousePos];
                foreach (var m in board.GenerateMoves(selectedPiece))
                {
                    if (board.DetectCheck(currentSide, m))
                    {
                        disallowedMoves.Add(m);
                    }
                    else
                    {
                        allowedMoves.Add(m);
                    }
                }
            }

            return move;
        }

        public void Draw(GameTime gameTime)
        {
            const string gridLetters = "abcdefgh";

            Rectangle rect;
            int size = Board.SIZE / 8;
            calculationTime += gameTime.ElapsedGameTime.TotalSeconds;

            //Draw board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? assetServer.GetTexture(TileType.White) : assetServer.GetTexture(TileType.Black);
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

                    if (parameters.ShowGrid)
                    {
                        spriteBatch.DrawString(assetServer.GetFont(18), gridLetters[x].ToString() + (8 - y), new Vector2(x * size, y * size), Color.Red);
                    }
                }
            }

            //Draw selected piece moves
            if (!selectedPiece.IsNull)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Selected), new Rectangle(selectedPiece.Position.X * size,
                    selectedPiece.Position.Y * size, size, size), Color.White * 0.5f);

                rect = new(0, 0, size, size);
                foreach (var move in allowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(assetServer.GetTexture(TileType.Allowed), rect, Color.White * 0.5f);
                }

                foreach (var move in disallowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(assetServer.GetTexture(TileType.Disallowed), rect, Color.White * 0.5f);
                }
            }

            //Draw when king in danger
            if (state == ChessState.WhiteCheck || state == ChessState.BlackCheck)
            {
                Side side = state == ChessState.WhiteCheck ? Side.White : Side.Black;
                var king = board.GetKing(side);
                rect = new(king.Position.X * size, king.Position.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.Danger), rect, Color.White * 0.5f);
            }

            //Draw previous move
            if (!move.IsNull)
            {
                rect = new(move.TargetPosition.X * size, move.TargetPosition.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.MoveHighlight), rect, Color.White * 0.5f);

                rect = new(move.Piece.Position.X * size, move.Piece.Position.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.MoveHighlight), rect, Color.White * 0.5f);
            }

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f),
                    (int)(size * 0.7f), (int)(size * 0.8f));

                spriteBatch.Draw(assetServer.GetTexture(piece.Data), rect, Color.White);
            }

            if (!PlayerTurn && calculationTime > 0.5)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Shading),
                    new Rectangle(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30, 120, 30), Color.White);

                spriteBatch.DrawString(assetServer.GetFont(26), "Calculating",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30), Color.Azure);
            }

            if (parameters.GameState != GameState.Running)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Shading), new Rectangle(0, 0, Board.SIZE, Board.SIZE), Color.White);
            }

            if (parameters.GameState == GameState.End)
            {
                spriteBatch.DrawString(assetServer.GetFont(24), Util.ReverseSide(currentSide).ToString() + " Victory",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 120), Color.AntiqueWhite);
            }
        }

        public void LoadBoardState()
        {
            if (parameters.PiecesData != null)
            {
                board.SetPieces(parameters.PiecesData);
                board.SetCastlingData(parameters.CastlingData[0], parameters.CastlingData[1]);
            }
        }

        public void SetCurrentSide(Side side)
        {
            currentSide = side;
        }

        public void EraseState()
        {
            parameters.PiecesData = null;
            parameters.Save();
        }

        public void SaveState()
        {
            parameters.CurrentSide = currentSide;
            parameters.PiecesData = board.GetPiecesData();
            parameters.CastlingData = board.GetCastlingData();
            parameters.Save();
        }
    }
}
