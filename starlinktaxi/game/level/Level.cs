using starlinktaxi.game.level.element;
using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
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

        private string title;
        private int missionCount;
        private Canvas root;

        public List<LevelElement> Elements { get; } = new List<LevelElement>();
        public List<element.Dock> Docks { get; } = new List<element.Dock>();
        public Gate Gate { get; }
        public Spaceman Spaceman { get; }

        public string Title { get => title; set { title = value; ControlPropertyChanged(); } }
        public LevelElement Spawnpoint { get; protected set; }
        public int MissionCount { get => missionCount; set { missionCount = value; ControlPropertyChanged(); } }
        public Canvas Root { get => root; set { root = value; ControlPropertyChanged(); } }

        public double Gravity { get; set; } = 0.0;

        public Mission CurrentMission { get; private set; }

        public Level(string levelName)
        {
            this.levelName = levelName;

            Uri uri = new Uri(getResource(LevelResource.XAML, "Resource.xaml"), UriKind.Relative);
            Stream stream = Application.GetResourceStream(uri).Stream;
            Root = XamlReader.Load(stream) as Canvas;
            stream.Close();

            Root.Width = GameUtil.ScreenWidth - 40;
            Root.Height = GameUtil.ScreenHeight - 140;

            foreach (FrameworkElement dock in (Root.FindName("Docks") as Canvas).Children)
            {
                CalculateParameters(dock);
                element.Dock element = new element.Dock(dock);
                Docks.Add(element);
            }

            foreach (FrameworkElement collidable in (Root.FindName("Collidable") as Canvas).Children)
            {
                CalculateParameters(collidable);
                CollidableLevelElement element = new CollidableLevelElement(collidable);                
                Elements.Add(element);
            }

            foreach (FrameworkElement notCollidable in (Root.FindName("NotCollidable") as Canvas).Children)
            {
                CalculateParameters(notCollidable);
                LevelElement element = new LevelElement(notCollidable);
                Elements.Add(element);
            }

            FrameworkElement gateElement = Root.FindName("Gate") as FrameworkElement;
            CalculateParameters(gateElement);
            Gate = new Gate(gateElement);

            FrameworkElement spacemanElement = Root.FindName("Spaceman") as FrameworkElement;
            CalculateParameters(spacemanElement);
            Spaceman = new Spaceman(spacemanElement);

            MissionCount = Docks.Count();
        }

        public void Start()
        {
            NewMission?.Invoke(SetupNextMission());
        }

        public string getResource(LevelResource resource, string name) 
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
            return path + name;
        }

        private void CalculateParameters(FrameworkElement element)
        {
            element.Width = GameUtil.ScreenWidth / 100 * element.Width;
            element.Height = GameUtil.ScreenHeight / 100 * element.Height;
            Canvas.SetLeft(element, GameUtil.ScreenWidth / 100 * Canvas.GetLeft(element));
            Canvas.SetTop(element, GameUtil.ScreenHeight / 100 * Canvas.GetTop(element));
        }

        private Mission SetupNextMission()
        {
            if(CurrentMission == null || CurrentMission.Type != MissionType.GATE)
            {
                Mission newMission = new Mission();
                newMission.Reward = new MissionReward(10, 15);
                if (MissionCount > 0)
                {
                    IMissionElement newDock;
                    do
                    {
                        newDock = Docks[GameUtil.Random.Next(Docks.Count())];
                    } while (CurrentMission != null && newDock == CurrentMission.Element);
                    newMission.Element = newDock;
                    ((Rectangle)((Grid)((LevelElement)newMission.Element).Root).Children[0]).Fill = Brushes.Green;
                }
                else
                {
                    newMission.Element = Gate;
                }
                newMission.Type = newMission.Element is Gate ? MissionType.GATE : CurrentMission != null && CurrentMission.Type == MissionType.PICKUP ? MissionType.TRANSPORT : MissionType.PICKUP;
                if(newMission.Type == MissionType.PICKUP)
                {
                    Spaceman.Spawn(newMission.Element as LevelElement);
                }
                CurrentMission = newMission;
                return CurrentMission;
            }
            CurrentMission = null;
            return null;
        }

        public void CompleteMission()
        {
            if (!CurrentMission.IsCompleted)
            {
                CurrentMission.IsCompleted = true;
                ((Rectangle)((Grid)((LevelElement)CurrentMission.Element).Root).Children[0]).Fill = Brushes.Red;
                if(CurrentMission.Type == MissionType.GATE)
                {
                    NewMission?.Invoke(SetupNextMission());
                } else if (CurrentMission.Type == MissionType.PICKUP)
                {
                    Spaceman.Despawn();
                    NewMission?.Invoke(SetupNextMission());
                }
                else if (CurrentMission.Type == MissionType.TRANSPORT)
                {
                    MissionCount--;
                    Spaceman.Spawn(CurrentMission.Element as element.Dock);
                    MissionCompleted?.Invoke(CurrentMission.Reward);
                    new Thread(() =>
                    {
                        Thread.Sleep(3000);
                        Root.Dispatcher.Invoke(new Action(() =>
                        {
                            Spaceman.Despawn();
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
