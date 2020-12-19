using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace starlinktaxi.game.level
{
    public class CollidableLevelElement : LevelElement, ICollidable
    {

        public Point Position { get; }
        public double SizeX { get; }
        public double SizeY { get; }

        public CollidableLevelElement(FrameworkElement root) : base(root)
        {
            Position = new Point(Canvas.GetLeft(root), Canvas.GetTop(root));
            SizeX = (root).Width;
            SizeY = (root).Height;
        }

        public Point? IsColliding(ICollidable with)
        {
            double top, bottom, left, right;
            if (with.Position.Y + with.SizeY > Position.Y && with.Position.Y < Position.Y + SizeY &&
               with.Position.X + with.SizeX > Position.X && with.Position.X < Position.X + SizeX)
            {
                top = with.Position.Y + with.SizeY - Position.Y;
                bottom = Position.Y + SizeY - with.Position.Y;
                left = with.Position.X + with.SizeX - Position.X;
                right = Position.X + SizeX - with.Position.X;

                double leftRight = left < right ? left * -1 : right;
                double topBottom = top < bottom ? top * -1: bottom;
                return new Point(Math.Abs(leftRight) > Math.Abs(topBottom) ? 0 : leftRight, Math.Abs(topBottom) > Math.Abs(leftRight) ? 0 : topBottom);
            }

            return null;
        }
    }
}
