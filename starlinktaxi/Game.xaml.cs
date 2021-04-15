using starlinktaxi.game;
using starlinktaxi.game.level;
using starlinktaxi.Properties;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using starlinktaxi.resource.level;
using System.Threading;

namespace starlinktaxi
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Window
    {

        private readonly GameHandler handler;

        public Game()
        {
            InitializeComponent();

            handler = new GameHandler();
            this.DataContext = handler;

            PreviewKeyDown += handler.OnKeyDown;
            PreviewKeyUp += handler.OnKeyUp;
        }

        public void New()
        {
            handler.DeleteSave();
            handler.NextLevel();
        }

        public void Load()
        {
            handler.InitFromSave();
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TextBlock)
            {
                TextBlock label = e.Source as TextBlock;

                if (label == Endgame)
                {
                    handler.SaveGame();
                    Close();
                } else if(label == Continue)
                {
                    handler.SetPause(false);
                } else if(label == BuyFix)
                {
                    if (handler.Spaceship.Health < 100)
                    {
                        handler.Money -= 1;
                        handler.Spaceship.Health += 5;
                    }
                }
                else if (label == BuyFuel)
                {
                    if (handler.Spaceship.Fuel < 100)
                    {
                        handler.Money -= 2;
                        handler.Spaceship.Fuel += 5;
                    }
                } else if (label == NextLevel)
                {
                    handler.CompletedLevelCount++;
                    handler.Money -= handler.NewLevelPrice;
                    handler.SetPause(false);
                    handler.NextLevel();
                }
            }
        }
    }
}
