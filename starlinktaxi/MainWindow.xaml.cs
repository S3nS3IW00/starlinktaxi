using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace starlinktaxi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            menu.MouseLeftButtonDown += OnLeftMouseDown;
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is Label)
            {
                Label label = e.Source as Label;

                if (label == newgame)
                {
                    Game game = new Game();
                    game.Show();
                    game.Closing += new CancelEventHandler(delegate (Object o, CancelEventArgs a)
                    {
                        Show();
                    });
                    Hide();
                }
            } else if(e.Source is Image && (e.Source as Image) == exit)
            {
                App.Current.Shutdown();
            }
        }
    }
}
