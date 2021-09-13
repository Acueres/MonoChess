using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoChess.Controllers;
using MonoChess.Models;

namespace MonoChess
{
    public enum ChessState
    {
        Opening,
        Running,
        Check,
        Stalemate
    }

    public class Chess
    {
        public static int ScreenWidth { get; set; }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D whiteTile;
        Texture2D blackTile;
        Texture2D goldTile;
        SpriteFont font;
        Dictionary<string, Texture2D> textures;

        AIController ai;
        PlayerController player;
        Board board = new();

        Sides turn;
        Sides playerSide;

        ChessState state;

        public Chess(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, SpriteFont font)
        {
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;
            this.textures = textures;
            this.font = font;

            ScreenWidth = (int)(graphics.PreferredBackBufferWidth / 8f);

            ai = new(board);
            player = new(board);

            whiteTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            Color[] data = new Color[50 * 50];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Beige;
            }
            whiteTile.SetData(data);

            blackTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Olive;
            }
            blackTile.SetData(data);

            goldTile = new Texture2D(graphics.GraphicsDevice, 50, 50);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Gold;
            }
            goldTile.SetData(data);

            Console.Write("Choose side, white (0) or black (1): ");
            playerSide = (Sides)Convert.ToInt32(Console.ReadLine());
        }

        public void Update()
        {
            IController current = turn == playerSide ? player : ai;
            Move move = current.NextMove(turn, state);

            if (!move.IsNull)
            {
                board.MakeMove(move);
                turn = turn == Sides.White ? Sides.Black : Sides.White;
                state = ChessState.Running;
            }
        }

        public void Draw()
        {
            Rectangle rect;
            MouseState ms = Mouse.GetState();
            var size = (int)(graphics.PreferredBackBufferWidth / 8f);

            //Draw tiles
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                    //shows coordinates for debugging
                    spriteBatch.DrawString(font, x + " " + y, new Vector2(x * size, y * size), Color.Red);
                }
            }

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                if (!player.DraggedPiece.IsNull && piece == player.DraggedPiece)
                {
                    continue;
                }

                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f),
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
                    rect.X = move.X * size;
                    rect.Y = move.Y * size;
                    spriteBatch.Draw(goldTile, rect, Color.White * 0.5f);
                }
            }

            Console.Write($"\rStatus: {state}");
        }
    }
}
