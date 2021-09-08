using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoChess.Models;

namespace MonoChess.Controllers
{
    interface IController
    {
        public Move NextMove(Sides side);
    }
}
