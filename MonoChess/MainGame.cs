using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace MonoChess
{
    public enum GameState : byte
    {
        MainMenu,
        SetupMenu,
        Running,
        Pause,
        End
    }

    public class MainGame : Game
    {
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Menu menu;
        ChessEngine chess;

        readonly GameParameters parameters;
        Task engineTask;
        KeyboardState previousKs;

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

            parameters = new GameParameters();
            previousKs = new KeyboardState();

            engineTask = Task.CompletedTask;
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

            chess = new ChessEngine(spriteBatch, parameters, assetServer);
            menu = new Menu(GraphicsDevice, chess, parameters, spriteBatch, assetServer, () => Exit());

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
                menu.Update();

                if (parameters.GameState == GameState.Running && engineTask.IsCompleted)
                {
                    engineTask = chess.Update();
                    await engineTask;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            chess.Draw(gameTime);
            menu.Draw();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CheckInput()
        {
            var ks = Keyboard.GetState();

            if (Util.KeyPressed(Keys.Escape, ks, previousKs))
            {
                if (parameters.GameState == GameState.Running)
                {
                    parameters.GameState = GameState.Pause;
                }
                else if (parameters.GameState == GameState.Pause)
                {
                    parameters.GameState = GameState.Running;
                }
            }

            else if (Util.KeyPressed(Keys.G, ks, previousKs))
            {
                parameters.ShowGrid ^= true;
            }

            previousKs = ks;
        }
    }
}
