﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpChess
{
    class Program
    {
        static void Main()
        {
            using Chess chess = new();
            chess.Run();
        }
    }
}
