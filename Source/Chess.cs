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
        Middle,
        Check
    }

    public class Chess
    {
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

        int turnCount;

        Sides currentSide;
        Sides playerSide;

        ChessState state;

        public Chess(GraphicsDevice graphics, SpriteBatch spriteBatch, GameParameters parameters,
            Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
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
            turnCount = 0;
            currentSide = Sides.White;
            playerController.SelectedPiece = Piece.Null;
            board.Reset();
        }

        public bool Update()
        {
            if (turnCount == 0)
            {
                playerSide = parameters.Side;
            }

            if (state == ChessState.Check && board.DetectCheckmate(currentSide))
            {
                return true;
            }

            IController controller = currentSide == playerSide || !parameters.SinglePlayer ? playerController : aiController;
            Move move = controller.NextMove(currentSide, state);

            if (!move.IsNull)
            {
                board.MakeMove(move);

                currentSide = currentSide == Sides.White ? Sides.Black : Sides.White;

                if (board.DetectCheck(currentSide))
                {
                    state = ChessState.Check;
                }
                else
                {
                    state = ChessState.Middle;
                }

                turnCount++;
            }

            return false;
        }

        public void Draw(GameState gameState)
        {
            Rectangle rect;
            var size = GameParameters.BOARD_WIDTH / 8;

            //Draw tiles
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                }
            }

            if (!playerController.SelectedPiece.IsNull)
            {
                spriteBatch.Draw(selectedTile, new Rectangle(playerController.SelectedPiece.Position.X * size,
                    playerController.SelectedPiece.Position.Y * size, size, size), Color.White * 0.5f);

                rect = new(0, 0, size, size);
                foreach (var move in playerController.AllowedMoves)
                {
                    rect.X = move.Position.X * size;
                    rect.Y = move.Position.Y * size;
                    spriteBatch.Draw(allowedTile, rect, Color.White * 0.5f);
                }

                foreach (var move in playerController.DisallowedMoves)
                {
                    rect.X = move.Position.X * size;
                    rect.Y = move.Position.Y * size;
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

            if (gameState != GameState.Running)
            {
                spriteBatch.Draw(shading, new Rectangle(0, 0, GameParameters.BOARD_WIDTH, GameParameters.BOARD_WIDTH), Color.White);
            }
        }
    }
}
