using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

using MonoChess.Controllers;
using MonoChess.Models;

namespace MonoChess
{
    public enum ChessState
    {
        Opening,
        Default,
        WhiteCheck,
        BlackCheck
    }

    public class Chess
    {
        readonly MainGame game;

        readonly SpriteBatch spriteBatch;
        readonly Texture2D whiteTile;
        readonly Texture2D blackTile;
        readonly Texture2D allowedTile;
        readonly Texture2D disallowedTile;
        readonly Texture2D selectedTile;
        readonly Texture2D shading;
        readonly Texture2D dangerTile;
        readonly Texture2D moveHighlightTile;
        readonly Dictionary<int, DynamicSpriteFont> fonts;
        readonly Dictionary<string, Texture2D> textures;
        readonly string[] filesChars = { "a", "b", "c", "d", "e", "f", "g", "h" };

        readonly GameParameters parameters;
        readonly AIController aiController;
        readonly PlayerController playerController;
        readonly Board board = new();

        Task<Move> nextMoveTask;
        Sides currentSide = Sides.White;
        ChessState state;
        Move move = Move.Null;
        bool waiting;
        bool PlayerTurn { get => currentSide == parameters.PlayerSide || !parameters.SinglePlayer; }
        double calculationTime;

        public Chess(MainGame game, GraphicsDevice graphics, SpriteBatch spriteBatch, GameParameters parameters,
            Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
            this.game = game;
            this.spriteBatch = spriteBatch;
            this.parameters = parameters;
            this.textures = textures;
            this.fonts = fonts;

            aiController = new AIController(board);
            playerController = new PlayerController(board);

            whiteTile = Util.GetColoredTexture(graphics, 50, 50, Color.LightGoldenrodYellow);
            blackTile = Util.GetColoredTexture(graphics, 50, 50, Color.Olive);
            allowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Green);
            disallowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.DarkRed);
            selectedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Blue);
            dangerTile = Util.GetColoredTexture(graphics, 50, 50, Color.Red);
            moveHighlightTile = Util.GetColoredTexture(graphics, 50, 50, Color.Gold);

            shading = Util.GetColoredTexture(graphics, 50, 50, Color.Black, 0.8f);
        }

        public void Reset()
        {
            playerController.Interrupt();
            nextMoveTask.Wait();

            currentSide = Sides.White;
            playerController.SelectedPiece = Piece.Null;
            move = Move.Null;
            board.SetPieces();
            waiting = false;
            state = ChessState.Opening;
        }

        public async Task Update()
        {
            if (waiting || game.State == GameState.End) return;

            if (board.DetectCheckmate(currentSide))
            {
                game.State = GameState.End;
                return;
            }

            if (board.DetectCheck(currentSide))
            {
                state = currentSide == Sides.White ? ChessState.WhiteCheck : ChessState.BlackCheck;
            }
            else
            {
                state = ChessState.Default;
            }

            waiting = true;
            IController controller = PlayerTurn ? playerController : aiController;
            calculationTime = 0;
            nextMoveTask = controller.NextMoveAsync(parameters, currentSide, state);
            move = await nextMoveTask;

            if (move.IsNull) return;

            board.MakeMove(move, out _);

            currentSide = Util.ReverseSide(currentSide);

            waiting = false;
        }

        public void Draw(GameState gameState, GameTime gameTime)
        {
            Rectangle rect;
            var size = Board.SIZE / 8;
            calculationTime += gameTime.ElapsedGameTime.TotalSeconds;

            //Draw board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

                    if (parameters.ShowGrid)
                    {
                        spriteBatch.DrawString(fonts[22], filesChars[x] + (8 - y), new Vector2(x * size, y * size), Color.Red);
                    }
                }
            }

            //Draw selected piece moves
            if (!playerController.SelectedPiece.IsNull)
            {
                spriteBatch.Draw(selectedTile, new Rectangle(playerController.SelectedPiece.Position.X * size,
                    playerController.SelectedPiece.Position.Y * size, size, size), Color.White * 0.5f);

                rect = new(0, 0, size, size);
                foreach (var move in playerController.AllowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(allowedTile, rect, Color.White * 0.5f);
                }

                foreach (var move in playerController.DisallowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(disallowedTile, rect, Color.White * 0.5f);
                }
            }

            //Draw when king in danger
            if (state == ChessState.WhiteCheck || state == ChessState.BlackCheck)
            {
                Sides side = state == ChessState.WhiteCheck ? Sides.White : Sides.Black;
                var king = board.GetKing(side);
                rect = new(king.Position.X * size, king.Position.Y * size, size, size);
                spriteBatch.Draw(dangerTile, rect, Color.White * 0.5f);
            }

            //Draw previous move
            if (!move.IsNull)
            {
                rect = new(move.TargetPosition.X * size, move.TargetPosition.Y * size, size, size);
                spriteBatch.Draw(moveHighlightTile, rect, Color.White * 0.5f);

                rect = new(move.Piece.Position.X * size, move.Piece.Position.Y * size, size, size);
                spriteBatch.Draw(moveHighlightTile, rect, Color.White * 0.5f);
            }

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f),
                    (int)(size * 0.7f), (int)(size * 0.8f));

                spriteBatch.Draw(textures[piece.Name], rect, Color.White);
            }

            if (waiting && !PlayerTurn && calculationTime > 0.5)
            {
                spriteBatch.Draw(shading,
                    new Rectangle(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30, 120, 30), Color.White);

                spriteBatch.DrawString(fonts[26], "Calculating",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30), Color.Azure);
            }

            if (gameState != GameState.Running)
            {
                spriteBatch.Draw(shading, new Rectangle(0, 0, Board.SIZE, Board.SIZE), Color.White);
            }

            if (gameState == GameState.End)
            {
                spriteBatch.DrawString(fonts[24], Util.ReverseSide(currentSide).ToString() + " Victory",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 120), Color.AntiqueWhite);
            }
        }

        public void LoadBoardState()
        {
            if (parameters.PiecesData != null)
            {
                board.SetPieces(parameters.PiecesData);
                board.CastlingData = parameters.CastlingData;
            }
        }

        public void EraseState()
        {
            parameters.PiecesData = null;
            parameters.Save();
        }

        public void SaveState()
        {
            parameters.PiecesData = board.GetPiecesData();
            parameters.CastlingData = board.CastlingData;
            parameters.Save();
        }
    }
}
