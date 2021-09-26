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
        Label background;

        MenuState state;
        MouseState previousMs = Mouse.GetState();
        bool abandonGame;


        public Menu(MainGame game, GraphicsDevice graphics, GameParameters parameters, SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures, Dictionary<int, DynamicSpriteFont> fonts)
        {
            this.spriteBatch = spriteBatch;

            var buttonBase = Util.GetColoredTexture(graphics, 50, 50, Color.Goldenrod);

            background = new Label()
            {
                Texture = Util.GetColoredTexture(graphics, 50, 50, Color.DarkRed),
                Rect = new(0, 0, GameParameters.BOARD_WIDTH, GameParameters.MENU_HEIGHT)
            };

            main = new List<IGUIElement>();

            Button singlePlayer = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH / 2 - 120, GameParameters.MENU_HEIGHT / 4, 120, 30),
                Text = "Single Player",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = true; state = MenuState.Setup; }
            };
            main.Add(singlePlayer);

            Button twoPlayers = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH / 2 + 30, GameParameters.MENU_HEIGHT / 4, 120, 30),
                Text = "Two Players",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { parameters.SinglePlayer = false; state = MenuState.InGame; game.State = GameState.Running; }
            };
            main.Add(twoPlayers);

            Button quit = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH - 90, GameParameters.MENU_HEIGHT / 4, 60, 30),
                Text = "Quit",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { game.Exit(); }
            };
            main.Add(quit);

            setup = new List<IGUIElement>();

            Label chooseSide = new()
            {
                Rect = new(30, GameParameters.MENU_HEIGHT / 8, 100, 30),
                Text = "Play as:",
                TextColor = Color.AntiqueWhite,
                Font = fonts[20]
            };
            setup.Add(chooseSide);

            Label sideBase = new()
            {
                Rect = new(55, 30, 50, 50),
                Texture = buttonBase
            };
            setup.Add(sideBase);

            Button side = new()
            {
                Texture = textures["w_king"],
                Rect = new(60, 35, 40, 40)
            };
            side.Action = () =>
            {
                parameters.Side = parameters.Side == Sides.White ? parameters.Side = Sides.Black : Sides.White;
                side.Texture = parameters.Side == Sides.White ? textures["w_king"] : textures["b_king"];
            };
            setup.Add(side);

            Button play = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH / 2 + 80, GameParameters.MENU_HEIGHT / 2, 60, 30),
                Text = "Play",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { state = MenuState.InGame; game.State = GameState.Running; }
            };
            setup.Add(play);

            Button back = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH - 80, GameParameters.MENU_HEIGHT / 2, 60, 30),
                Text = "Back",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { state = MenuState.Main; }
            };
            setup.Add(back);

            inGame = new List<IGUIElement>();
            Button abandon = new()
            {
                Texture = buttonBase,
                Rect = new(GameParameters.BOARD_WIDTH / 2 + 80, GameParameters.MENU_HEIGHT / 2, 150, 30),
                Text = "Abandon game",
                TextColor = Color.Black,
                Font = fonts[22],
                Action = () => { abandonGame = true; }
            };
            inGame.Add(abandon);
        }

        public void Draw()
        {
            background.Draw(spriteBatch);

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
            else
            {
                foreach (var el in inGame)
                {
                    el.Draw(spriteBatch);
                }
            }
        }

        public bool Update()
        {
            abandonGame = false;

            MouseState ms = Mouse.GetState();
            if (state == MenuState.Main)
            {
                foreach (var el in main)
                {
                    el.Update(ms.Position, Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }
            else if (state == MenuState.Setup)
            {
                foreach (var el in setup)
                {
                    el.Update(ms.Position, Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }
            else
            {
                foreach (var el in inGame)
                {
                    el.Update(ms.Position, Util.MouseClicked(ms.LeftButton, previousMs.LeftButton));
                }
            }

            previousMs = ms;

            return abandonGame;
        }
    }
}
