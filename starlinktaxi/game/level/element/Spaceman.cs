using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace starlinktaxi.game.level.element
{
    public class Spaceman : LevelElement, ISpawnable
    {

        private DoubleAnimation spawnAnimation = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(500), From = 0.0, To = 1.0 }, despawnAnimation = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(500), From = 1.0, To = 0.0 };

        public Spaceman(FrameworkElement root) : base(root)
        {
            
        }

        public void Spawn(LevelElement element)
        {
            Root.BeginAnimation(Canvas.LeftProperty, null);
            Canvas.SetLeft(Root, Canvas.GetLeft(element.Root) + element.Root.Width / 2 - (Root.Width / 2));
            Canvas.SetTop(Root, Canvas.GetTop(element.Root) - Root.Height);

            Root.BeginAnimation(UIElement.OpacityProperty, spawnAnimation);
        }

        public void Despawn()
        {
            Root.BeginAnimation(UIElement.OpacityProperty, despawnAnimation);
        }
    }
}
