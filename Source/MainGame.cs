using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;


namespace MonoChess
{
    public enum Pieces
    {
        Null,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum Sides
    {
        White,
        Black
    }

    public enum GameState
    {
        MainMenu,
        SetupMenu,
        Running,
        Pause,
        Endgame
    }

    public class MainGame : Game
    {
        public GameState State { get; set; }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Menu menu;
        Chess chess;
        GameParameters parameters = new();

        KeyboardState prevKs = Keyboard.GetState();


        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferMultiSampling = true,  
                IsFullScreen = false
            };

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Content.RootDirectory = "assets";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = Board.SIZE;
            graphics.PreferredBackBufferHeight = Board.SIZE;
            graphics.ApplyChanges();

            byte[] ttfData = File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf");
            FontSystem fs = new();
            fs.AddFont(ttfData);

            var fonts = Enumerable.Range(12, 21).ToDictionary(x => x, x => fs.GetFont(x));

            spriteBatch = new SpriteBatch(GraphicsDevice);

            var textures = LoadTextures();

            chess = new Chess(this, GraphicsDevice, spriteBatch, parameters, textures, fonts);
            menu = new Menu(this, GraphicsDevice, chess, parameters, spriteBatch, textures, fonts);

            base.Initialize();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override async void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                CheckInput();
                menu.Update(State);

                if (State == GameState.Running)
                {
                    await chess.Update();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            chess.Draw(State);
            menu.Draw(State);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Dictionary<string, Texture2D> LoadTextures()
        {
            var paths = Directory.GetFiles("assets/pieces/", ".").ToArray();
            Dictionary<string, Texture2D> textures = new();

            foreach (var path in paths)
            {
                var textureName = path.Split("/", 2)[^1].Split("/")[1].Split(".")[0];

                textures.Add(textureName, Content.Load<Texture2D>("pieces/" + textureName));
            }

            return textures;
        }

        private void CheckInput()
        {
            var ks = Keyboard.GetState();

            if (Util.KeyPressed(Keys.Escape, ks, prevKs))
            {
                if (State == GameState.Running)
                {
                    State = GameState.Pause;
                }
                else if (State == GameState.Pause)
                {
                    State = GameState.Running;
                }
            }

            else if (Util.KeyPressed(Keys.G, ks, prevKs))
            {
                parameters.ShowGrid ^= true;
            }

            prevKs = ks;
        }
    }
}
