using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.GUI;
using MonoChess.Algorithms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;


namespace MonoChess
{
    class Menu
    {
        public void ToMain() => state = MenuState.Main;

        enum MenuState
        {
            Main,
            Setup,
            InGame
        }

        SpriteBatch spriteBatch;
        List<IGUIElement> main;
        List<IGUIElement> setup;
        List<IGUIElement> inGame;

        MenuState state;
        MouseState previousMs = Mouse.GetState();
        bool abandonGame;

        readonly int[] depthLevels = { 1, 2, 3, 4 };
        readonly Dictionary<int, string> difficultyLevels = new()
        {
            [1] = "Very Easy",
            [2] = "Easy",
            [3] = "Normal",
            [4] = "Hard"
        };


        public Menu(MainGame game, GraphicsDevice graphics, GameParameters parameters, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
            this.spriteBatch = spriteBatch;

            var buttonBase = Util.GetColoredTexture(graphics, 50, 50, Color.Goldenrod);
            Button.BaseTexture = buttonBase;
            Button.Highlight = Util.GetColoredTexture(graphics, 50, 50, Color.White);

            CreateMainMenu(game, parameters, fonts);
            CreateSetupMenu(game, parameters, fonts, textures, buttonBase);
            CreateInGameMenu(game, fonts);
        }

        public void Draw(GameState gameState)
        {
            if (state == MenuState.Main)
            {
                foreach (var el in main)
                {
                    el.Draw(spriteBatch);
                }
            }
            else if (state == MenuState.Setup)
            {
                foreach (var el in setup)
                {
                    el.Draw(spriteBatch);
                }
            }
            else if (state == MenuState.InGame && gameState == GameState.Pause)
            {
                foreach (var el in inGame)
                {
                    el.Draw(spriteBatch);
                }
            }
        }

        public bool Update(GameState gameState)
        {
            abandonGame = false;

            MouseState ms = Mouse.GetState();

            if (state == MenuState.Main)
            {
                foreach (var el in main)
                {
                    el.Update(Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }
            else if (state == MenuState.Setup)
            {
                foreach (var el in setup)
                {
                    el.Update(Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }
            else if (state == MenuState.InGame && gameState == GameState.Pause)
            {
                foreach (var el in inGame)
                {
                    el.Update(Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }

            previousMs = ms;

            return abandonGame;
        }

        private void CreateMainMenu(MainGame game, GameParameters parameters, Dictionary<int, DynamicSpriteFont> fonts)
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

            Button singlePlayer = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Single Player",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = true; state = MenuState.Setup; }
            };
            main.Add(singlePlayer);

            Button twoPlayers = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Two Players",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = false; state = MenuState.InGame; game.State = GameState.Running; }
            };
            main.Add(twoPlayers);

            Button quit = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2, 120, 30),
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
                parameters.AlgorithmType = (Algorithm)Util.GetNextIndex((int)parameters.AlgorithmType, (int)Algorithm.Count);
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
                    state = MenuState.InGame; 
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
                Action = () => { state = MenuState.Main; }
            };
            setup.Add(back);
        }

        private void CreateInGameMenu(MainGame game, Dictionary<int, DynamicSpriteFont> fonts)
        {
            inGame = new List<IGUIElement>();

            Label pauseLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Pause",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            inGame.Add(pauseLabel);

            Button abandon = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Leave match",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { abandonGame = true; }
            };
            inGame.Add(abandon);

            Button @return = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2, 120, 30),
                Text = "Return",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.State = GameState.Running; }
            };
            inGame.Add(@return);
        }
    }
}
