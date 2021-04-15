using starlinktaxi.game.level.element;
using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace starlinktaxi.game.level
{

    public delegate void MissionCompletedListener(MissionReward reward);
    public delegate void NewMissionListener(Mission mission);

    public class Level : Bindable
    {

        public event MissionCompletedListener MissionCompleted;
        public event NewMissionListener NewMission;

        private readonly string levelName;
        public string LevelName { get => levelName; }

        private string title;
        private int newLevelPrice;
        private Canvas root;

        public List<LevelElement> Elements { get; } = new List<LevelElement>();
        public LinkedList<element.Dock> Docks { get; } = new LinkedList<element.Dock>();
        public List<Shop> Shops { get; } = new List<Shop>();
        public Gate Gate { get; }
        public Spaceman Spaceman { get; }

        public string Title { get => title; set { title = value; ControlPropertyChanged(); } }
        public LevelElement Spawnpoint { get; protected set; }
        public Canvas Root { get => root; set { root = value; ControlPropertyChanged(); } }

        public double Gravity { get; set; } = 0.0;

        public Mission CurrentMission { get; set; }

        private DropShadowEffect highlightEffect = new DropShadowEffect() { BlurRadius = 50, ShadowDepth = 0, Opacity = 1, Color = Colors.White };

        public Level(string levelName)
        {
            this.levelName = levelName;

            Uri uri = GetResource(LevelResource.XAML, "Resource.xaml");
            Stream stream = Application.GetResourceStream(uri).Stream;
            Root = XamlReader.Load(stream) as Canvas;
            stream.Close();

            Root.Width = GameUtil.ScreenWidth - 40;
            Root.Height = GameUtil.ScreenHeight - 140;

            double stock = 1280 * 768;
            double current = GameUtil.ScreenWidth * GameUtil.ScreenHeight;
            double percentage = (stock > current ? current / stock : stock / current) * 100.0;
            int difference = (int)(16 / 100.0 * percentage);
            int fontSize = stock > current ? 16 - difference : 16 + difference;

            foreach (FrameworkElement dock in (Root.FindName("Docks") as Canvas).Children)
            {
                CalculateParameters(dock);
                element.Dock element = new element.Dock(dock);
                Docks.AddLast(element);

                ((dock as Panel).Children[1] as TextBlock).FontSize = fontSize;
            }

            foreach (FrameworkElement shop in (Root.FindName("Shops") as Canvas).Children)
            {
                CalculateParameters(shop);
                Shop element = new Shop(shop);
                Shops.Add(element);

                ((shop as Panel).Children[1] as TextBlock).FontSize = fontSize;
            }

            foreach (FrameworkElement collidable in (Root.FindName("Collidable") as Canvas).Children)
            {
                CalculateParameters(collidable);
                CollidableLevelElement element = new CollidableLevelElement(collidable);
                Elements.Add(element);
            }

            object notCollidables = Root.FindName("NotCollidable");
            if (notCollidables != null)
            {
                foreach (FrameworkElement notCollidable in (notCollidables as Canvas).Children)
                {
                    CalculateParameters(notCollidable);
                    LevelElement element = new LevelElement(notCollidable);
                    Elements.Add(element);
                }
            }

            FrameworkElement gateElement = Root.FindName("Gate") as FrameworkElement;
            CalculateParameters(gateElement);
            Gate = new Gate(gateElement);

            FrameworkElement spacemanElement = Root.FindName("Spaceman") as FrameworkElement;
            CalculateParameters(spacemanElement);
            Spaceman = new Spaceman(spacemanElement);
        }

        public void LoadTextures()
        {
            foreach (element.Dock dock in Docks)
            {
                SetTexture((dock.Root as Panel).Children[0] as Rectangle, "dock.jpg");
            }
            foreach(LevelElement element in Elements)
            {
                SetTexture(element.Root as Rectangle, (element is CollidableLevelElement ? "" : "not") + "collidable.jpg");
            }
            foreach (Shop shop in Shops)
            {
                SetTexture((shop.Root as Panel).Children[0] as Rectangle, "shop.jpg");
            }

            // Set background
            Image bg = new Image()
            {
                Source = new BitmapImage(GetResource(LevelResource.TEXTURE, "bg.jpg"))
            };
            Root.Background = new VisualBrush(bg)
            {
                TileMode = TileMode.None, Stretch = Stretch.Fill
            };
        }

        public void Start()
        {
            NewMission?.Invoke(SetupNextMission());
        }

        public Uri GetResource(LevelResource resource, string name)
        {
            string path = null;
            switch(resource)
            {
                case LevelResource.XAML:
                    path = "resource/level/" + levelName + "/";
                    break;
                case LevelResource.TEXTURE:
                    path = "resource/level/" + levelName + "/texture/";
                    break;
            }
            return new Uri(path + name, UriKind.Relative);
        }

        private void CalculateParameters(FrameworkElement element)
        {
            element.Width = GameUtil.ScreenWidth / 100 * element.Width;
            element.Height = GameUtil.ScreenHeight / 100 * element.Height;
            Canvas.SetLeft(element, GameUtil.ScreenWidth / 100 * Canvas.GetLeft(element));
            Canvas.SetTop(element, GameUtil.ScreenHeight / 100 * Canvas.GetTop(element));
        }

        private void SetTexture(Rectangle rectangle, string textureName)
        {
            Image image = new Image()
            {
                Source = new BitmapImage(GetResource(LevelResource.TEXTURE, textureName))
            };
            rectangle.Fill = new VisualBrush(image)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(new Point(0, 0), new Point(128, 128)),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        private Mission SetupNextMission()
        {
            Mission newMission = new Mission();
            newMission.Reward = new MissionReward(10, 15);
            IMissionElement newDock;
            do
            {
                newDock = Docks.ElementAt(GameUtil.Random.Next(Docks.Count()));
            } while (CurrentMission != null && newDock == CurrentMission.Element);
            newMission.Element = newDock;
            (newDock as element.Dock).Root.Effect = highlightEffect;
            newMission.Type = CurrentMission != null && CurrentMission.Type == MissionType.PICKUP ? MissionType.TRANSPORT : MissionType.PICKUP;
            if (newMission.Type == MissionType.PICKUP)
            {
                Spaceman.Spawn(newMission.Element as LevelElement);
            }
            CurrentMission = newMission;
            return CurrentMission;
        }

        public void OverrideMission(Mission mission)
        {
            if (CurrentMission != null)
            {
                (CurrentMission.Element as element.Dock).Root.Effect = null;
                Spaceman.Despawn();
            }

            (mission.Element as element.Dock).Root.Effect = highlightEffect;
            if (mission.Type == MissionType.PICKUP)
            {
                Spaceman.Spawn(mission.Element as LevelElement);
            }
            CurrentMission = mission;
            NewMission?.Invoke(mission);
        }

        public void CompleteMission()
        {
            if (!CurrentMission.IsCompleted)
            {
                CurrentMission.IsCompleted = true;
                (CurrentMission.Element as element.Dock).Root.Effect = null;
                if (CurrentMission.Type == MissionType.PICKUP)
                {
                    NewMission?.Invoke(SetupNextMission());
                }
                else if (CurrentMission.Type == MissionType.TRANSPORT)
                {
                    MissionCompleted?.Invoke(CurrentMission.Reward);
                    new Thread(() =>
                    {
                        Thread.Sleep(2500);
                        Root.Dispatcher.Invoke(new Action(() =>
                        {
                            Spaceman.Despawn();
                        }));
                        Thread.Sleep(500);
                        Root.Dispatcher.Invoke(new Action(() =>
                        {
                            NewMission?.Invoke(SetupNextMission());
                        }));
                    }).Start();
                }
            }
        }
    }

    public enum LevelResource
    {
        XAML, TEXTURE
    }

    public class LevelResources : Bindable
    {
        private string xaml;

        public string Xaml { get => xaml; set { xaml = value; ControlPropertyChanged(); } }
    }

}
