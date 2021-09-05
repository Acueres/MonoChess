using System;
using System.Collections.Generic;
using System.Linq;


namespace MonoChess.Models
{
    public struct Move
    {
        public Piece Piece { get; set; }
        public Position Position { get; set; }
        public bool IsNull
        {
            get
            {
                return Piece.IsNull;
            }
        }

        public Move(Piece piece, Position position)
        {
            Piece = piece;
            Position = position;
        }
    }
}
