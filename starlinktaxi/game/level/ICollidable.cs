using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace starlinktaxi.game.level
{
    public interface ICollidable
    {

        Point Position { get; }
        double SizeX { get; }
        double SizeY { get; }

    }
}
