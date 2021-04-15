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

        private Grid currentContent;

        public static MainController MainController { get; } = new MainController();        

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainController;

            currentContent = menubuttons;
            MainController.CheckSaveGame();
            main.MouseLeftButtonDown += OnLeftMouseDown;
        }

        private void ChangeContent(Grid to)
        {
            currentContent.Visibility = Visibility.Hidden;
            to.Visibility = Visibility.Visible;
            currentContent = to;
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
                    game.New();
                    game.Closing += new CancelEventHandler(delegate (Object o, CancelEventArgs a)
                    {
                        Show();
                    });
                    Hide();
                } 
                else if (label == @continue)
                {
                    Game game = new Game();
                    game.Show();
                    game.Load();
                    game.Closing += new CancelEventHandler(delegate (Object o, CancelEventArgs a)
                    {
                        Show();
                    });
                    Hide();
                }
                else if (label == about)
                {
                    ChangeContent(aboutpage);
                }
            } else if(e.Source is Image && (e.Source as Image) == exit)
            {
                if (currentContent == menubuttons)
                {
                    App.Current.Shutdown();
                } else
                {
                    ChangeContent(menubuttons);
                }
            }
        }
    }
}
