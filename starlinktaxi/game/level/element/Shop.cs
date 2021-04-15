using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace starlinktaxi.game.level.element
{
    public class Shop : CollidableLevelElement
    {
        public TextBlock TextBlock { get; }

        public Shop(FrameworkElement root) : base(root)
        {
            TextBlock = (TextBlock)(root as Panel).Children[1];
        }
    }
}
