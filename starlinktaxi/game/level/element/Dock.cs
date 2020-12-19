using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace starlinktaxi.game.level.element
{
    public class Dock : CollidableLevelElement, IMissionElement
    {

        public TextBlock TextBlock { get; }

        public Dock(FrameworkElement root) : base (root)
        {
            TextBlock = (TextBlock)(root as Panel).Children[1];
        }
    }
}
