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

        Sides turn = Sides.White;

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
        }

        public void Update()
        {
            if (turn == Sides.Black)
            {
                Move move = ai.NextMove(Sides.Black);
                board.MakeMove(move);
                turn = Sides.White;
            }
            else
            {
                Move move = player.NextMove(Sides.White);

                if (!move.IsNull)
                {
                    board.MakeMove(move);
                    turn = Sides.Black;
                }
            }
        }

        public void Draw()
        {
            Rectangle rect;
            MouseState ms = Mouse.GetState();
            var size = (int)(graphics.PreferredBackBufferWidth / 8f);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Position pos = new(x, y);
                    Texture2D tile = (x + y) % 2 == 0 ? whiteTile : blackTile;
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);
                    //shows coordinates for debugging
                    spriteBatch.DrawString(font, x + " " + y, new Vector2(x * size, y * size), Color.Red);

                    if (!board[pos].IsNull)
                    {
                        if (!player.DraggedPiece.IsNull && board[pos] == player.DraggedPiece)
                        {
                            continue;
                        }

                        rect.Height = (int)(rect.Height * 0.8f);
                        rect.Width = (int)(rect.Width * 0.7f);
                        rect.Y += (int)(rect.Height * 0.2f);
                        rect.X += (int)(rect.Width * 0.2f);

                        spriteBatch.Draw(textures[board[pos].Name], rect, Color.White);
                    }
                }
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
        }
    }
}
