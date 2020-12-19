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

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Label)
            {
                Label label = e.Source as Label;

                if (label == Endgame)
                {
                    Close();
                } else if(label == Continue)
                {
                    handler.SetPause(false);
                }
            }
        }
    }
}
