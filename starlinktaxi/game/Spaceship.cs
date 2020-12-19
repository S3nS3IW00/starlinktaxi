using starlinktaxi.game.level;
using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace starlinktaxi
{

    public delegate void HealthChangeListener(double from, double to);
    public delegate void OutOfFuelListener();

    public class Spaceship : Bindable, ICollidable, ISpawnable
    {

        public event HealthChangeListener HealthChanged;
        public event OutOfFuelListener OutOfFuel;

        private string model;
        private double x, y, speedX, speedY;
        private int scaleX;
        private double health = 100, fuel = 100;

        public string Model { get => model; set { model = value; ControlPropertyChanged(); } }
        public double X { get => x; set { x = value; ControlPropertyChanged(); } }
        public double Y { get => y; set { y = value; ControlPropertyChanged(); } }
        public double SpeedX { get => speedX; set { speedX = value; ControlPropertyChanged(); } }
        public double SpeedY { get => speedY; set { speedY = value; ControlPropertyChanged(); } }
        public int ScaleX { get => scaleX; set { scaleX = value; ControlPropertyChanged(); } }
        public double Health
        {
            get => health;
            set
            {
                double from = health;
                double to = value < 0 ? 0 : value;
                health = to;
                ControlPropertyChanged();
                HealthChanged?.Invoke(from, to);
            }
        }
        public double Fuel
        {
            get => fuel;
            set
            {
                fuel = value < 0 ? 0 : value;
                ControlPropertyChanged();
                if(fuel == 0)
                {
                    OutOfFuel?.Invoke();
                }
            }
        }

        public Point Position { get => new Point(X - (ScaleX == -1 ? 100 : 0), Y); }
        public double SizeX => 100;
        public double SizeY => 20;

        public void Spawn(LevelElement element)
        {
            X = Canvas.GetLeft(element.Root) + element.Root.Width / 2 - (SizeX / 2);
            Y = Canvas.GetTop(element.Root) - SizeY;
        }
    }
}
