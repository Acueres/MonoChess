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
        MainGame game;

        SpriteBatch spriteBatch;
        Texture2D whiteTile;
        Texture2D blackTile;
        Texture2D allowedTile;
        Texture2D disallowedTile;
        Texture2D selectedTile;
        Texture2D shading;
        Dictionary<int, DynamicSpriteFont> fonts;
        Dictionary<string, Texture2D> textures;

        GameParameters parameters;
        AIController aiController;
        PlayerController playerController;
        Board board = new();

        Sides currentSide;
        ChessState state;
        bool waiting;


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
            allowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Gold);
            disallowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Red);
            selectedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Blue);
            shading = Util.GetColoredTexture(graphics, 50, 50, Color.Black, 0.8f);
        }

        public void Reset()
        {
            waiting = false;
            currentSide = Sides.White;
            state = ChessState.Opening;
            playerController.SelectedPiece = Piece.Null;
            board.SetPieces();
        }

        public async Task Update()
        {
            if (waiting || game.State == GameState.Endgame) return;

            if (board.DetectCheckmate(currentSide))
            {
                game.State = GameState.Endgame;
                return;
            }

            waiting = true;
            IController controller = currentSide == parameters.PlayerSide || !parameters.SinglePlayer ? playerController : aiController;
            Move move = await controller.NextMoveAsync(parameters, currentSide, state);

            if (move.IsNull)
            {
                waiting = false;
                return;
            }

            board.MakeMove(move, out _);

            currentSide = Util.ReverseSide(currentSide);

            if (board.DetectCheck(currentSide))
            {
                state = currentSide == Sides.White ? ChessState.WhiteCheck : ChessState.BlackCheck;
            }
            else
            {
                state = ChessState.Default;
            }

            waiting = false;
        }

        public void Draw(GameState gameState)
        {
            Rectangle rect;
            var size = Board.SIZE / 8;

            //Draw tiles
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

                    if (parameters.ShowGrid)
                    {
                        spriteBatch.DrawString(fonts[22], x + " " + y, new Vector2(x * size, y * size), Color.Red);
                    }
                }
            }

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

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f),
                    (int)(size * 0.7f), (int)(size * 0.8f));

                spriteBatch.Draw(textures[piece.Name], rect, Color.White);
            }

            if (waiting)
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

            if (gameState == GameState.Endgame)
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
            parameters.Save();
        }
    }
}
