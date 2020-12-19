using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace starlinktaxi.game.level
{
    public class LevelElement
    {
        
        public FrameworkElement Root { get; }

        public LevelElement(FrameworkElement root)
        {
            Root = root;
        }

    }
}
