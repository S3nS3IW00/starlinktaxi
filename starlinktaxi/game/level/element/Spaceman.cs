using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace starlinktaxi.game.level.element
{
    public class Spaceman : LevelElement, ISpawnable
    {

        public Spaceman(FrameworkElement root) : base(root)
        {
            
        }

        public void Spawn(LevelElement element)
        {
            Canvas.SetLeft(Root, (Canvas.GetLeft(element.Root) + element.Root.Width / 2) - (Root.Width / 2));
            Canvas.SetTop(Root, Canvas.GetTop(element.Root) - Root.Height);
            Root.Visibility = Visibility.Visible;
        }

        public void Despawn()
        {
            Root.Visibility = Visibility.Hidden;
        }
    }
}
