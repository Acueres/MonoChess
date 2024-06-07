using MonoChess;
using MonoChess.Models;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void TestBoard()
        {
            var board = new Board(new Piece[]
            {
                new(Pieces.Pawn, Sides.White, new(1, 6)),
                new(Pieces.Pawn, Sides.Black, new(0, 3))
            });

            Move move1 = new(board[new(1, 6)], new(1, 4));
            board.MakeMove(move1, out _);
            Assert.True(board[new(1, 4)].Type == move1.Piece.Type && board[new(1, 4)].Side == move1.Piece.Side);
            Assert.True(board[new(1, 6)].IsNull);

            Move move2 = new(board[new(0, 3)], new(1, 4));
            board.MakeMove(move2, out var removed);
            Assert.True(removed.Type == move1.Piece.Type && removed.Side == move1.Piece.Side);
            Assert.True(board[new(1, 4)].Type == move2.Piece.Type && board[new(1, 4)].Side == move2.Piece.Side);

            board.ReverseMove(move2, removed);
            board.ReverseMove(move1, Piece.Null);
            Assert.True(board[new(1, 4)].IsNull);
            Assert.True(board[new(1, 6)] == move1.Piece);
            Assert.True(board[new(0, 3)] == move2.Piece);
        }

        [Fact]
        public void TestCastling()
        {
            var board = new Board(new Piece[]
            {
                new(Pieces.King, Sides.White, new(4, 7)),
                new(Pieces.Rook, Sides.White, new(0, 7)),
                new(Pieces.Rook, Sides.White, new(7, 7))
            });

            //testing castling to the left
            Move move1 = new(board[new(4, 7)], new(0, 7));
            board.MakeMove(move1, out var removed);
            Assert.True(removed.Type == Pieces.Rook);
            Assert.True(board[new(2, 7)].Type == Pieces.King);
            Assert.True(board[new(3, 7)].Type == Pieces.Rook);
            Assert.True(!board.GetCastling(Sides.White));

            board.ReverseMove(move1, removed);
            Assert.True(board[new(2, 7)].Type == Pieces.Null);
            Assert.True(board[new(3, 7)].Type == Pieces.Null);
            Assert.True(board[new(4, 7)].Type == Pieces.King);
            Assert.True(board[new(0, 7)].Type == Pieces.Rook);
            Assert.True(board.GetCastling(Sides.White));

            //testing castling to the right
            Move move2 = new(board[new(4, 7)], new(7, 7));
            board.MakeMove(move2, out removed);
            Assert.True(removed.Type == Pieces.Rook);
            Assert.True(board[new(6, 7)].Type == Pieces.King);
            Assert.True(board[new(5, 7)].Type == Pieces.Rook);
            Assert.True(!board.GetCastling(Sides.White));

            board.ReverseMove(move2, removed);
            Assert.True(board[new(6, 7)].Type == Pieces.Null);
            Assert.True(board[new(5, 7)].Type == Pieces.Null);
            Assert.True(board[new(4, 7)].Type == Pieces.King);
            Assert.True(board[new(7, 7)].Type == Pieces.Rook);
            Assert.True(board.GetCastling(Sides.White));

            //testing that no castling is possible after moving king
            Move move3 = new(board[new(4, 7)], new(4, 6));
            board.MakeMove(move3, out _);
            Assert.True(!board.GetCastling(Sides.White));
            Move move4 = new(board[new(4, 6)], new(4, 7));
            board.MakeMove(move4, out _);
            Assert.True(!board.GetCastling(Sides.White));

            board.ReverseMove(move4, Piece.Null);
            board.ReverseMove(move3, Piece.Null);
            Assert.True(board.GetCastling(Sides.White));

            //testing that no castling is possible after moving both rooks
            Move move5 = new(board[new(0, 7)], new(0, 6));
            board.MakeMove(move5, out _);
            Assert.True(board.GetCastling(Sides.White));

            Move move6 = new(board[new(7, 7)], new(7, 6));
            board.MakeMove(move6, out _);
            Assert.True(!board.GetCastling(Sides.White));

            Move move7 = new(board[new(7, 6)], new(7, 7));
            board.MakeMove(move7, out _);
            Assert.True(!board.GetCastling(Sides.White));

            Move move8 = new(board[new(0, 6)], new(0, 7));
            board.MakeMove(move8, out _);
            Assert.True(!board.GetCastling(Sides.White));

            board.ReverseMove(move8, Piece.Null);
            board.ReverseMove(move7, Piece.Null);
            board.ReverseMove(move6, Piece.Null);
            Assert.True(board.GetCastling(Sides.White));
            board.ReverseMove(move5, Piece.Null);
            Assert.True(board.GetCastling(Sides.White));

            board[new(7, 7)] = Piece.Null;
            Assert.True(board.GetCastling(Sides.White));
            Move move9 = new(board[new(0, 7)], new(1, 7));
            board.MakeMove(move9, out _);
            Assert.True(!board.GetCastling(Sides.White));
            board.ReverseMove(move9, Piece.Null);
            Assert.True(board.GetCastling(Sides.White));
        }

        [Fact]
        public void TestPromotion()
        {
            Piece whitePawn = new(Pieces.Pawn, Sides.White, new(0, 1));
            Piece blackPawn = new(Pieces.Pawn, Sides.Black, new(0, 6));

            var board = new Board(new Piece[]
            {
                whitePawn,
                blackPawn
            });

            Move whitePawnMove = new(whitePawn, new(0, 0));
            board.MakeMove(whitePawnMove, out _);
            Assert.True(board[new(0, 0)].Type == Pieces.Queen);

            board.ReverseMove(whitePawnMove, Piece.Null);
            Assert.True(board[new(0, 1)].Type == Pieces.Pawn);

            Move blackPawnMove = new(blackPawn, new(0, 7));
            board.MakeMove(blackPawnMove, out _);
            Assert.True(board[new(0, 7)].Type == Pieces.Queen);

            board.ReverseMove(blackPawnMove, Piece.Null);
            Assert.True(board[new(0, 6)].Type == Pieces.Pawn);
        }
    }
}