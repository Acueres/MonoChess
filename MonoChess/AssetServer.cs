using FontStashSharp;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using MonoChess.Enums;
using System;

namespace MonoChess
{
    public class AssetServer(ContentManager content, GraphicsDevice graphics)
    {
        readonly ContentManager content = content;
        readonly GraphicsDevice graphics = graphics;

        readonly Dictionary<int, Texture2D> pieces = [];
        readonly Dictionary<TileType, Texture2D> tiles = [];
        readonly Dictionary<int, DynamicSpriteFont> fonts = [];

        public void Load()
        {
            LoadTextures();
            MakeTileTextures();
            LoadFonts();
        }

        public Texture2D GetTexture(int id) => pieces[id];
        public Texture2D GetTexture(PieceType pieceType, Side side) => pieces[Math.Sign((int)side) == 1 ? (int)pieceType : -(int)pieceType];
        public Texture2D GetTexture(TileType tileType) => tiles[tileType];
        public DynamicSpriteFont GetFont(int fontSize) => fonts[fontSize];

        private void LoadTextures()
        {
            var pieceTypes = Enum.GetValues(typeof(PieceType)).Cast<PieceType>().ToArray();
            foreach (PieceType pieceType in pieceTypes)
            {
                if (pieceType == PieceType.Null) continue;

                pieces.Add((int)pieceType, content.Load<Texture2D>($"Pieces/w_{pieceType.ToString().ToLower()}"));
                pieces.Add(-(int)pieceType, content.Load<Texture2D>($"Pieces/b_{pieceType.ToString().ToLower()}"));
            }
        }

        private void MakeTileTextures()
        {
            var whiteTile = Util.GetColoredTexture(graphics, 50, 50, Color.LightGoldenrodYellow);
            var blackTile = Util.GetColoredTexture(graphics, 50, 50, Color.Olive);
            var allowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Green);
            var disallowedTile = Util.GetColoredTexture(graphics, 50, 50, Color.DarkRed);
            var selectedTile = Util.GetColoredTexture(graphics, 50, 50, Color.Blue);
            var dangerTile = Util.GetColoredTexture(graphics, 50, 50, Color.Red);
            var moveHighlightTile = Util.GetColoredTexture(graphics, 50, 50, Color.Gold);
            var shading = Util.GetColoredTexture(graphics, 50, 50, Color.Black, 0.8f);

            tiles.Add(TileType.White, whiteTile);
            tiles.Add(TileType.Black, blackTile);
            tiles.Add(TileType.Allowed, allowedTile);
            tiles.Add(TileType.Disallowed, disallowedTile);
            tiles.Add(TileType.Selected, selectedTile);
            tiles.Add(TileType.Danger, dangerTile);
            tiles.Add(TileType.MoveHighlight, moveHighlightTile);
            tiles.Add(TileType.Shading, shading);
        }

        private void LoadFonts()
        {
            byte[] ttfData = File.ReadAllBytes(@"Assets\Fonts\LiberationSans-Regular.ttf");
            FontSystem fs = new();
            fs.AddFont(ttfData);

            var fontSizes = Enumerable.Range(12, 21);
            foreach (int fontSize in fontSizes)
            {
                fonts.Add(fontSize, fs.GetFont(fontSize));
            }
        }
    }
}
