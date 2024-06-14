using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using MonoChess.Enums;
using MonoChess.Models;

namespace MonoChess
{
    public class MainGame : Game
    {
        public GameState State { get; set; }

        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Menu menu;
        ChessEngine chess;
        readonly GameParameters parameters = new();

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

            Content.RootDirectory = "Assets";
        }

        protected override void Initialize()
        {
            const int SIZE = 504;

            graphics.PreferredBackBufferWidth = SIZE;
            graphics.PreferredBackBufferHeight = SIZE;

            graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            AssetServer assetServer = new(Content, graphics.GraphicsDevice);
            assetServer.Load();

            chess = new ChessEngine(this, spriteBatch, parameters, assetServer);
            menu = new Menu(this, GraphicsDevice, chess, parameters, spriteBatch, assetServer);

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

            chess.Draw(State, gameTime);
            menu.Draw(State);

            spriteBatch.End();

            base.Draw(gameTime);
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
