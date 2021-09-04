using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;


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

    public class MainGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Chess chess;

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
            graphics.PreferredBackBufferWidth = 504;
            graphics.PreferredBackBufferHeight = 504;
            graphics.ApplyChanges();

            var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf"), 25, 1024, 1024,
                new[]
                {
                    CharacterRange.BasicLatin
                }
            );

            SpriteFont font = fontBakeResult.CreateSpriteFont(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            var textures = LoadTextures();

            chess = new Chess(graphics, spriteBatch, textures, font);

            base.Initialize();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                chess.Update();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            chess.Draw();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Dictionary<string, Texture2D> LoadTextures()
        {
            var paths = Directory.GetFiles("Assets/", ".").ToArray();
            Dictionary<string, Texture2D> textures = new();

            foreach (var path in paths)
            {
                var textureName = path.Split("/")[1].Split(".")[0];

                textures.Add(textureName, Content.Load<Texture2D>(textureName));
            }

            return textures;
        }
    }
}
