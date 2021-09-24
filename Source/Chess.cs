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
        Texture2D goldTile;
        Texture2D shading;
        DynamicSpriteFont font;
        Dictionary<string, Texture2D> textures;

        GameParameters parameters;
        AIController ai;
        PlayerController player;
        Board board = new();

        int turnCount;

        Sides currentSide;
        Sides playerSide;

        ChessState state;

        public Chess(GraphicsDevice graphics, SpriteBatch spriteBatch, GameParameters parameters,
            Dictionary<string, Texture2D> textures, DynamicSpriteFont font)
        {
            this.spriteBatch = spriteBatch;
            this.parameters = parameters;
            this.textures = textures;
            this.font = font;

            ai = new AIController(board);
            player = new PlayerController(board);

            whiteTile = Util.GetColoredTexture(graphics, 50, 50, Color.Beige);
            blackTile = Util.GetColoredTexture(graphics, 50, 50, Color.Olive);
            goldTile = Util.GetColoredTexture(graphics, 50, 50, Color.Gold);
            shading = Util.GetColoredTexture(graphics, 50, 50, Color.Black, 0.8f);
        }

        public void Reset()
        {
            turnCount = 0;
            currentSide = Sides.White;
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

            IController controller = currentSide == playerSide ? player : ai;
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
            MouseState ms = Mouse.GetState();
            var size = GameParameters.BOARD_WIDTH / 8;

            //Draw tiles
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size + GameParameters.MENU_HEIGHT, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                    //shows coordinates for debugging
                    spriteBatch.DrawString(font, x + " " + y, new Vector2(x * size, y * size + GameParameters.MENU_HEIGHT), Color.Red);
                }
            }

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                if (!player.DraggedPiece.IsNull && piece == player.DraggedPiece)
                {
                    continue;
                }

                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f) + GameParameters.MENU_HEIGHT,
                    (int)(size * 0.7f), (int)(size * 0.8f));

                spriteBatch.Draw(textures[piece.Name], rect, Color.White);
            }

            //Draw dragged piece at cursor's position
            if (!player.DraggedPiece.IsNull)
            {
                rect = new(ms.X, ms.Y, (int)(size * 0.7f), (int)(size * 0.8f));
                rect.X -= (int)(0.5f * rect.Width);
                rect.Y -= (int)(0.5f * rect.Height);

                spriteBatch.Draw(textures[player.DraggedPiece.Name], rect, Color.White);

                rect = new(0, 0, size, size);
                foreach (var move in player.AllowedMoves)
                {
                    rect.X = move.Position.X * size;
                    rect.Y = move.Position.Y * size + GameParameters.MENU_HEIGHT;
                    spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
                }
            }

            if (gameState != GameState.Running)
            {
                spriteBatch.Draw(shading, new Rectangle(0, GameParameters.MENU_HEIGHT, GameParameters.BOARD_WIDTH, GameParameters.BOARD_WIDTH), Color.White);
            }
        }
    }
}
