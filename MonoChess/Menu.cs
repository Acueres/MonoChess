using System.Collections.Generic;

using MonoChess.Enums;
using MonoChess.GUI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;


namespace MonoChess
{
    public class Menu
    {
        SpriteBatch spriteBatch;
        List<IGUIElement> main;
        List<IGUIElement> setup;
        List<IGUIElement> pause;
        List<IGUIElement> endgame;
        Dictionary<GameState, List<IGUIElement>> grids;

        MouseState previousMs = Mouse.GetState();

        readonly int[] depthLevels = { 1, 2, 3, 4, 5 };
        readonly Dictionary<int, string> difficultyLevels = new()
        {
            [1] = "Very Easy",
            [2] = "Easy",
            [3] = "Normal",
            [4] = "Hard",
            [5] = "Very Hard"
        };

        const int nAlgorithms = 3;

        public Menu(MainGame game, GraphicsDevice graphics, Chess chess, GameParameters parameters, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
            this.spriteBatch = spriteBatch;

            var buttonBase = Util.GetColoredTexture(graphics, 50, 50, Color.Goldenrod);
            Button.BaseTexture = buttonBase;
            Button.Highlight = Util.GetColoredTexture(graphics, 50, 50, Color.White);

            CreateMainMenu(game, chess, parameters, fonts);
            CreateSetupMenu(game, parameters, fonts, textures, buttonBase);
            CreateInGameMenu(game, chess, fonts);
            CreateEndgameMenu(game, chess, fonts);

            grids = new Dictionary<GameState, List<IGUIElement>>()
            {
                [GameState.MainMenu] = main,
                [GameState.SetupMenu] = setup,
                [GameState.Pause] = pause,
                [GameState.End] = endgame
            };
        }

        public void Draw(GameState state)
        {
            if (state == GameState.Running) return;

            foreach (var el in grids[state])
            {
                el.Draw(spriteBatch);
            }
        }

        public void Update(GameState state)
        {
            if (state == GameState.Running) return;

            MouseState ms = Mouse.GetState();

            foreach (var el in grids[state])
            {
                el.Update(Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
            }

            previousMs = ms;
        }

        private void CreateMainMenu(MainGame game, Chess chess, GameParameters parameters, Dictionary<int, DynamicSpriteFont> fonts)
        {
            main = new List<IGUIElement>();

            Label chessLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Chess",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            main.Add(chessLabel);

            Button resume = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Resume",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => 
                {
                    if (parameters.Load())
                    {
                        chess.LoadBoardState();
                        chess.SetCurrentSide(parameters.CurrentSide);
                        game.State = GameState.Running;
                    }
                }
            };
            main.Add(resume);

            Button singlePlayer = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Single Player",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = true; game.State = GameState.SetupMenu; }
            };
            main.Add(singlePlayer);

            Button twoPlayers = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 20, 120, 30),
                Text = "Two Players",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = false; game.State = GameState.Running; game.State = GameState.Running; }
            };
            main.Add(twoPlayers);

            Button quit = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 60, 120, 30),
                Text = "Quit Game",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.Exit(); }
            };
            main.Add(quit);
        }

        private void CreateSetupMenu(MainGame game, GameParameters parameters, Dictionary<int, DynamicSpriteFont> fonts,
            Dictionary<string, Texture2D> textures, Texture2D buttonBase)
        {
            setup = new List<IGUIElement>();

            Label setupLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Setup",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            setup.Add(setupLabel);

            Label chooseSide = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Play as",
                TextColor = Color.AntiqueWhite,
                Font = fonts[22]
            };
            setup.Add(chooseSide);

            Rectangle sideRect = new(Board.SIZE / 2 - 20, Board.SIZE / 2 - 70, 40, 40);
            Label sideBase = new()
            {
                Rect = sideRect,
                Texture = buttonBase
            };
            setup.Add(sideBase);

            Button side = new()
            {
                Texture = textures["w_king"],
                Rect = sideRect
            };
            side.Action = () =>
            {
                parameters.PlayerSide = parameters.PlayerSide == Sides.White ? parameters.PlayerSide = Sides.Black : Sides.White;
                side.Texture = parameters.PlayerSide == Sides.White ? textures["w_king"] : textures["b_king"];
            };
            setup.Add(side);

            Label chooseAlgorithm = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 10, 120, 30),
                Text = "Algorithm",
                TextColor = Color.AntiqueWhite,
                Font = fonts[22]
            };
            setup.Add(chooseAlgorithm);

            Button algorithm = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 20, 120, 30),
                Text = "AlphaBeta",
                TextColor = Color.Black,
                Font = fonts[22]
            };
            algorithm.Action = () =>
            {
                parameters.AlgorithmType = (AlgorithmType)Util.GetNextIndex((int)parameters.AlgorithmType, nAlgorithms);
                algorithm.Text = parameters.AlgorithmType.ToString();
            };
            setup.Add(algorithm);

            Label chooseDifficulty = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 70, 120, 30),
                Text = "Difficulty",
                TextColor = Color.AntiqueWhite,
                Font = fonts[22]
            };
            setup.Add(chooseDifficulty);

            Button difficulty = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 100, 120, 30),
                Text = difficultyLevels[parameters.Depth],
                TextColor = Color.Black,
                Font = fonts[22]
            };
            difficulty.Action = () =>
            {
                parameters.Depth = depthLevels[Util.GetNextIndex(parameters.Depth - 1, depthLevels.Length)];
                difficulty.Text = difficultyLevels[parameters.Depth];
            };
            setup.Add(difficulty);

            Button play = new()
            {
                Rect = new(Board.SIZE / 2 - 65, Board.SIZE / 2 + 150, 60, 30),
                Text = "Play",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => 
                { 
                    game.State = GameState.Running; 
                    Mouse.SetPosition(Board.SIZE / 2, Board.SIZE / 2);
                }
            };
            setup.Add(play);

            Button back = new()
            {
                Rect = new(Board.SIZE / 2 + 5, Board.SIZE / 2 + 150, 60, 30),
                Text = "Back",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.State = GameState.MainMenu; }
            };
            setup.Add(back);
        }

        private void CreateInGameMenu(MainGame game, Chess chess,
            Dictionary<int, DynamicSpriteFont> fonts)
        {
            pause = new List<IGUIElement>();

            Label menuLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Menu",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            pause.Add(menuLabel);

            Button abandon = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Abandon",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () =>
                {
                    chess.EraseState();
                    chess.Reset();
                    game.State = GameState.MainMenu;
                }
            };
            pause.Add(abandon);

            Button pauseButton = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Pause",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => 
                {
                    chess.SaveState();
                    chess.Reset();
                    game.State = GameState.MainMenu;
                }
            };
            pause.Add(pauseButton);

            Button @return = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2, 120, 30),
                Text = "Return",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.State = GameState.Running; }
            };
            pause.Add(@return);
        }

        private void CreateEndgameMenu(MainGame game, Chess chess, Dictionary<int, DynamicSpriteFont> fonts)
        {
            endgame = new List<IGUIElement>();

            Button abandon = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Main Menu",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () =>
                {
                    chess.EraseState();
                    chess.Reset();
                    game.State = GameState.MainMenu;
                }
            };
            endgame.Add(abandon);
        }
    }
}
