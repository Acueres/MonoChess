using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.GUI;
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


        public Menu(MainGame game, GraphicsDevice graphics, GameParameters parameters, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
            this.spriteBatch = spriteBatch;

            var buttonBase = Util.GetColoredTexture(graphics, 50, 50, Color.Goldenrod);
            Button.BaseTexture = buttonBase;
            Button.Highlight = Util.GetColoredTexture(graphics, 50, 50, Color.White);

            main = new List<IGUIElement>();

            Label chessLabel = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 150, 120, 30),
                Text = "Chess",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            main.Add(chessLabel);

            Button singlePlayer = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 100, 120, 30),
                Text = "Single Player",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = true; state = MenuState.Setup; }
            };
            main.Add(singlePlayer);

            Button twoPlayers = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 60, 120, 30),
                Text = "Two Players",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = false; state = MenuState.InGame; game.State = GameState.Running; }
            };
            main.Add(twoPlayers);

            Button quit = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2, 120, 30),
                Text = "Quit Game",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.Exit(); }
            };
            main.Add(quit);

            setup = new List<IGUIElement>();

            Label setupLabel = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 150, 120, 30),
                Text = "Setup",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            setup.Add(setupLabel);

            Label chooseSide = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 100, 120, 30),
                Text = "Play as",
                TextColor = Color.AntiqueWhite,
                Font = fonts[22]
            };
            setup.Add(chooseSide);

            Rectangle sideRect = new(GameParameters.BOARD_WIDTH / 2 - 20, GameParameters.BOARD_WIDTH / 2 - 70, 40, 40);
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
                parameters.Side = parameters.Side == Sides.White ? parameters.Side = Sides.Black : Sides.White;
                side.Texture = parameters.Side == Sides.White ? textures["w_king"] : textures["b_king"];
            };
            setup.Add(side);

            Button play = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 65, GameParameters.BOARD_WIDTH / 2 + 60, 60, 30),
                Text = "Play",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { state = MenuState.InGame; game.State = GameState.Running; }
            };
            setup.Add(play);

            Button back = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 + 5, GameParameters.BOARD_WIDTH / 2 + 60, 60, 30),
                Text = "Back",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { state = MenuState.Main; }
            };
            setup.Add(back);

            inGame = new List<IGUIElement>();

            Label pauseLabel = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 150, 120, 30),
                Text = "Pause",
                TextColor = Color.AntiqueWhite,
                Font = fonts[32]
            };
            inGame.Add(pauseLabel);

            Button abandon = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2 - 60, 120, 30),
                Text = "Leave match",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { abandonGame = true; }
            };
            inGame.Add(abandon);

            Button @return = new()
            {
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 60, GameParameters.BOARD_WIDTH / 2, 120, 30),
                Text = "Return",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.State = GameState.Running; }
            };
            inGame.Add(@return);
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
    }
}
