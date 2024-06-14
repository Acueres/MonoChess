using System.Collections.Generic;

using MonoChess.Enums;
using MonoChess.GUI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using MonoChess.Models;


namespace MonoChess
{
    public class Menu
    {
        readonly SpriteBatch spriteBatch;

        List<IGUIElement> main;
        List<IGUIElement> setup;
        List<IGUIElement> pause;
        List<IGUIElement> endgame;

        readonly Dictionary<GameState, List<IGUIElement>> grids;

        MouseState previousMs = Mouse.GetState();

        readonly int[] depthLevels = [1, 2, 3, 4, 5];
        readonly Dictionary<int, string> difficultyLevels = new()
        {
            [1] = "Very Easy",
            [2] = "Easy",
            [3] = "Normal",
            [4] = "Hard",
            [5] = "Very Hard"
        };

        const int nAlgorithms = 3;

        public Menu(MainGame game, GraphicsDevice graphics, ChessEngine chess, GameParameters parameters, SpriteBatch spriteBatch, AssetServer assetServer)
        {
            this.spriteBatch = spriteBatch;

            var buttonBase = Util.GetColoredTexture(graphics, 50, 50, Color.Goldenrod);
            Button.BaseTexture = buttonBase;
            Button.Highlight = Util.GetColoredTexture(graphics, 50, 50, Color.White);

            InitMainMenu(game, chess, parameters, assetServer);
            InitSetupMenu(game, parameters, assetServer, buttonBase);
            InitInGameMenu(game, chess, assetServer);
            InitEndgameMenu(game, chess, assetServer);

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

        private void InitMainMenu(MainGame game, ChessEngine chess, GameParameters parameters, AssetServer assetServer)
        {
            main = [];

            Label chessLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Chess",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(32)
            };
            main.Add(chessLabel);

            Button resume = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Resume",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
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
                Font = assetServer.GetFont(22),
                Action = () => { parameters.SinglePlayer = true; game.State = GameState.SetupMenu; }
            };
            main.Add(singlePlayer);

            Button twoPlayers = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 20, 120, 30),
                Text = "Two Players",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
                Action = () => { parameters.SinglePlayer = false; game.State = GameState.Running; game.State = GameState.Running; }
            };
            main.Add(twoPlayers);

            Button quit = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 60, 120, 30),
                Text = "Quit Game",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
                Action = () => { game.Exit(); }
            };
            main.Add(quit);
        }

        private void InitSetupMenu(MainGame game, GameParameters parameters, AssetServer assetServer, Texture2D buttonBase)
        {
            setup = [];

            Label setupLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Setup",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(32)
            };
            setup.Add(setupLabel);

            Label chooseSide = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Play as",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(22)
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
                Texture = assetServer.GetTexture(PieceType.King, Side.White),
                Rect = sideRect
            };
            side.Action = () =>
            {
                parameters.PlayerSide = parameters.PlayerSide == Side.White ? parameters.PlayerSide = Side.Black : Side.White;
                side.Texture = parameters.PlayerSide == Side.White ? assetServer.GetTexture(PieceType.King, Side.White) : assetServer.GetTexture(PieceType.King, Side.Black);
            };
            setup.Add(side);

            Label chooseAlgorithm = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 10, 120, 30),
                Text = "Algorithm",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(22)
            };
            setup.Add(chooseAlgorithm);

            Button algorithm = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 20, 120, 30),
                Text = "AlphaBeta",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22)
            };
            algorithm.Action = () =>
            {
                parameters.AlgorithmType = (AlgorithmType)GetNextIndex((int)parameters.AlgorithmType, nAlgorithms);
                algorithm.Text = parameters.AlgorithmType.ToString();
            };
            setup.Add(algorithm);

            Label chooseDifficulty = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 70, 120, 30),
                Text = "Difficulty",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(22)
            };
            setup.Add(chooseDifficulty);

            Button difficulty = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 + 100, 120, 30),
                Text = difficultyLevels[parameters.Depth],
                TextColor = Color.Black,
                Font = assetServer.GetFont(22)
            };
            difficulty.Action = () =>
            {
                parameters.Depth = depthLevels[GetNextIndex(parameters.Depth - 1, depthLevels.Length)];
                difficulty.Text = difficultyLevels[parameters.Depth];
            };
            setup.Add(difficulty);

            Button play = new()
            {
                Rect = new(Board.SIZE / 2 - 65, Board.SIZE / 2 + 150, 60, 30),
                Text = "Play",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
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
                Font = assetServer.GetFont(22),
                Action = () => { game.State = GameState.MainMenu; }
            };
            setup.Add(back);
        }

        private void InitInGameMenu(MainGame game, ChessEngine chess, AssetServer assetServer)
        {
            pause = [];

            Label menuLabel = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 150, 120, 30),
                Text = "Menu",
                TextColor = Color.AntiqueWhite,
                Font = assetServer.GetFont(32)
            };
            pause.Add(menuLabel);

            Button abandon = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 100, 120, 30),
                Text = "Abandon",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
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
                Font = assetServer.GetFont(22),
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
                Font = assetServer.GetFont(22),
                Action = () => { game.State = GameState.Running; }
            };
            pause.Add(@return);
        }

        private void InitEndgameMenu(MainGame game, ChessEngine chess, AssetServer assetServer)
        {
            endgame = [];

            Button abandon = new()
            {
                Rect = new(Board.SIZE / 2 - 60, Board.SIZE / 2 - 60, 120, 30),
                Text = "Main Menu",
                TextColor = Color.Black,
                Font = assetServer.GetFont(22),
                Action = () =>
                {
                    chess.EraseState();
                    chess.Reset();
                    game.State = GameState.MainMenu;
                }
            };
            endgame.Add(abandon);
        }

        static int GetNextIndex(int index, int end)
        {
            if (index == end - 1)
            {
                return 0;
            }

            return ++index;
        }
    }
}
