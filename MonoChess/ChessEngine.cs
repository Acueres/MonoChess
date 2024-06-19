using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

using MonoChess.Enums;
using MonoChess.Controllers;
using MonoChess.Models;

namespace MonoChess
{
    public class ChessEngine
    {
        readonly AssetServer assetServer;
        readonly SpriteBatch spriteBatch;

        readonly string[] filesChars = ["a", "b", "c", "d", "e", "f", "g", "h"];

        readonly GameParameters parameters;
        readonly AIController aiController;
        readonly PlayerController playerController;
        readonly Board board = new();

        Task<Move> nextMoveTask;
        Side currentSide = Side.White;
        ChessState state;
        Move move = Move.Null;
        bool waiting;
        bool PlayerTurn { get => currentSide == parameters.PlayerSide || !parameters.SinglePlayer; }
        double calculationTime;

        public ChessEngine(SpriteBatch spriteBatch, GameParameters parameters, AssetServer assetServer)
        {
            this.spriteBatch = spriteBatch;
            this.parameters = parameters;
            this.assetServer = assetServer;

            aiController = new AIController(board);
            playerController = new PlayerController(board);
        }

        public void Reset()
        {
            playerController.Interrupt();
            nextMoveTask.Wait();

            currentSide = Side.White;
            playerController.SelectedPiece = Piece.Null;
            move = Move.Null;
            board.SetPieces();
            waiting = false;
            state = ChessState.Opening;
        }

        public async Task Update()
        {
            if (waiting || parameters.GameState == GameState.End) return;

            if (board.DetectCheckmate(currentSide))
            {
                parameters.GameState = GameState.End;
                return;
            }

            if (board.DetectCheck(currentSide))
            {
                state = currentSide == Side.White ? ChessState.WhiteCheck : ChessState.BlackCheck;
            }
            else
            {
                state = ChessState.Default;
            }

            waiting = true;
            IController controller = PlayerTurn ? playerController : aiController;
            calculationTime = 0;
            nextMoveTask = controller.NextMoveAsync(parameters, currentSide, state);
            move = await nextMoveTask;

            if (move.IsNull) return;

            board.MakeMove(move, out _);

            currentSide = Util.ReverseSide(currentSide);

            waiting = false;
        }

        public void Draw(GameTime gameTime)
        {
            Rectangle rect;
            var size = Board.SIZE / 8;
            calculationTime += gameTime.ElapsedGameTime.TotalSeconds;

            //Draw board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Texture2D tile = (x + y) % 2 == 0 ? assetServer.GetTexture(TileType.White) : assetServer.GetTexture(TileType.Black);
                    rect = new(x * size, y * size, size, size);
                    spriteBatch.Draw(tile, rect, Color.White);

                    if (parameters.ShowGrid)
                    {
                        spriteBatch.DrawString(assetServer.GetFont(22), filesChars[x] + (8 - y), new Vector2(x * size, y * size), Color.Red);
                    }
                }
            }

            //Draw selected piece moves
            if (!playerController.SelectedPiece.IsNull)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Selected), new Rectangle(playerController.SelectedPiece.Position.X * size,
                    playerController.SelectedPiece.Position.Y * size, size, size), Color.White * 0.5f);

                rect = new(0, 0, size, size);
                foreach (var move in playerController.AllowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(assetServer.GetTexture(TileType.Allowed), rect, Color.White * 0.5f);
                }

                foreach (var move in playerController.DisallowedMoves)
                {
                    rect.X = move.TargetPosition.X * size;
                    rect.Y = move.TargetPosition.Y * size;
                    spriteBatch.Draw(assetServer.GetTexture(TileType.Disallowed), rect, Color.White * 0.5f);
                }
            }

            //Draw when king in danger
            if (state == ChessState.WhiteCheck || state == ChessState.BlackCheck)
            {
                Side side = state == ChessState.WhiteCheck ? Side.White : Side.Black;
                var king = board.GetKing(side);
                rect = new(king.Position.X * size, king.Position.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.Danger), rect, Color.White * 0.5f);
            }

            //Draw previous move
            if (!move.IsNull)
            {
                rect = new(move.TargetPosition.X * size, move.TargetPosition.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.MoveHighlight), rect, Color.White * 0.5f);

                rect = new(move.Piece.Position.X * size, move.Piece.Position.Y * size, size, size);
                spriteBatch.Draw(assetServer.GetTexture(TileType.MoveHighlight), rect, Color.White * 0.5f);
            }

            //Draw pieces
            foreach (var piece in board.GetPieces())
            {
                rect = new((int)(piece.Position.X * size + size * 0.2f), (int)(piece.Position.Y * size + size * 0.2f),
                    (int)(size * 0.7f), (int)(size * 0.8f));

                spriteBatch.Draw(assetServer.GetTexture(piece.Data), rect, Color.White);
            }

            if (waiting && !PlayerTurn && calculationTime > 0.5)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Shading),
                    new Rectangle(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30, 120, 30), Color.White);

                spriteBatch.DrawString(assetServer.GetFont(26), "Calculating",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 30), Color.Azure);
            }

            if (parameters.GameState != GameState.Running)
            {
                spriteBatch.Draw(assetServer.GetTexture(TileType.Shading), new Rectangle(0, 0, Board.SIZE, Board.SIZE), Color.White);
            }

            if (parameters.GameState == GameState.End)
            {
                spriteBatch.DrawString(assetServer.GetFont(24), Util.ReverseSide(currentSide).ToString() + " Victory",
                    new Vector2(Board.SIZE / 2 - 60, Board.SIZE / 2 - 120), Color.AntiqueWhite);
            }
        }

        public void LoadBoardState()
        {
            if (parameters.PiecesData != null)
            {
                board.SetPieces(parameters.PiecesData);
                board.SetCastlingData(parameters.CastlingData[0], parameters.CastlingData[1]);
            }
        }

        public void SetCurrentSide(Side side)
        {
            currentSide = side;
        }

        public void EraseState()
        {
            parameters.PiecesData = null;
            parameters.Save();
        }

        public void SaveState()
        {
            parameters.CurrentSide = currentSide;
            parameters.PiecesData = board.GetPiecesData();
            parameters.CastlingData = board.GetCastlingData();
            parameters.Save();
        }
    }
}
