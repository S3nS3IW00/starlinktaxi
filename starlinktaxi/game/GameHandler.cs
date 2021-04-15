using starlinktaxi.game.level;
using starlinktaxi.game.level.element;
using starlinktaxi.resource.level;
using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.Json;

namespace starlinktaxi.game
{
    public class GameHandler : Bindable
    {

        private readonly List<Key> holding = new List<Key>();
        private readonly List<Key> disabledControls = new List<Key>();
        public Timer Timer { get; }
        private Timer remainingSecondTimer;
        private bool waiting = false;

        private bool showExit = true, loading = false;
        public bool ShowExit { get => showExit; set { showExit = value; ControlPropertyChanged(); } }
        public bool IsLoading { get => loading; set { loading = value; ControlPropertyChanged(); } }

        private Cursor cursor;
        private bool paused = false, canTogglePause = true, shopping = false, lockShop = false, isGateMenu = false, lockGate = false;
        private string pauseTitle;

        public bool IsPaused { get => paused; set { paused = value; ControlPropertyChanged(); } }
        public bool IsShopping { get => shopping; set { shopping = value; ControlPropertyChanged(); } }
        public bool IsGateMenu { get => isGateMenu; set { isGateMenu = value; ControlPropertyChanged(); } }
        public bool CanTogglePause { get => canTogglePause; set { canTogglePause = value; ControlPropertyChanged(); } }
        public Cursor Cursor { get => cursor; set { cursor = value; ControlPropertyChanged(); } }
        public string PauseTitle { get => pauseTitle; set { pauseTitle = value; ControlPropertyChanged(); } }

        public Spaceship Spaceship { get; set; } = new Spaceship() { X = 0, Y = 0, ScaleX = -1, Model = "resource/spaceship/spaceship.png" };

        private Level currentLevel;
        private int completedLevelCount = 0;
        public Level CurrentLevel { get => currentLevel; set { currentLevel = value; ControlPropertyChanged(); } }
        public int CompletedLevelCount { get => completedLevelCount; set { completedLevelCount = value; ControlPropertyChanged(); } }

        private double money = 0, newLevelPrice = 0.0;
        private int remainingSeconds = 60;
        private string missionTitle;
        public double Money { get => money; set { money = value; ControlPropertyChanged(); } }
        public double NewLevelPrice { get => newLevelPrice; set { newLevelPrice = value; ControlPropertyChanged(); } }
        public int RemainingSeconds { get => remainingSeconds; set { remainingSeconds = value; ControlPropertyChanged(); } }
        public string MissionTitle { get => missionTitle; set { missionTitle = value; ControlPropertyChanged(); } }

        private DoubleAnimation enterAnimation = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(1500) }, exitAnimation = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(1500) };

        public bool DoSaveGame { get; set; } = true;

        public GameHandler()
        {
            Cursor = Cursors.None;

            Spaceship.HealthChanged += (from, to) =>
            {
                if(to == 0)
                {
                    EndGame();
                }
            };
            Spaceship.OutOfFuel += () =>
            {
                EndGame();
            };

            remainingSecondTimer = new Timer() { Interval = 1000 };
            remainingSecondTimer.Elapsed += (sender, e) =>
            {
                if (RemainingSeconds > 0)
                {
                    if(!waiting) RemainingSeconds--;
                }
                else
                {
                    EndGame();
                }
            };

            FreezeSpaceship(true);
            Timer = new Timer() { Interval = 1 };
            Timer.Elapsed += OnTick;
        }

        public void LoadLevel(Level level)
        {
            CurrentLevel = level;

            Spaceship.Spawn(CurrentLevel.Spawnpoint);

            CurrentLevel.NewMission += (mission) =>
            {
                if (mission.Type == MissionType.PICKUP)
                {
                    MissionTitle = "MENJ AZ UTASHOZ";
                }
                else if (mission.Type == MissionType.TRANSPORT)
                {
                    MissionTitle = "MENJ A KIJELÖLT HELYRE";
                }
                FreezeSpaceship(false);
                waiting = false;
            };

            CurrentLevel.MissionCompleted += (reward) =>
            {
                Money += reward.Money;
                RemainingSeconds += reward.Seconds;
                MissionTitle = "JUTALOM: " + reward.Seconds + " MÁSODPERC ÉS $" + reward.Money;
            };
            
            CurrentLevel.LoadTextures();
            CurrentLevel.Start();

            IsLoading = false;
            remainingSecondTimer.Enabled = true;
            Timer.Enabled = true;
        }

        public Task NextLevel()
        {
            Timer.Enabled = false;
            remainingSecondTimer.Enabled = false;
            IsLoading = true;

            Task loadTask = null;
            loadTask = new Task(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    switch (completedLevelCount)
                    {
                        case 0:
                            Level level1 = new Level1();
                            level1.Root.Dispatcher.Invoke(() =>
                            {
                                LoadLevel(level1);

                                NewLevelPrice = 50;
                                Spaceship.Health = 100;
                                Spaceship.Fuel = 100;
                            });
                            break;
                        case 1:
                            Level level2 = new Level2();
                            level2.Root.Dispatcher.Invoke(() =>
                            {
                                LoadLevel(level2);

                                NewLevelPrice = 100;
                                Spaceship.Health = 100;
                                Spaceship.Fuel = 100;
                            });
                            break;
                        case 2:
                            Level level3 = new Level3();
                            level3.Root.Dispatcher.Invoke(() =>
                            {
                                LoadLevel(level3);

                                NewLevelPrice = 150;
                                Spaceship.Health = 100;
                                Spaceship.Fuel = 100;
                            });
                            break;
                        default:
                            EndGame();
                            IsLoading = false;
                            break;
                    }
                });
            });
            if (loadTask != null)
            {
                loadTask.Start();
            }
            return loadTask;
        }

        public void SetPause(bool pause)
        {
            IsPaused = pause;
            Timer.Enabled = !pause;
            remainingSecondTimer.Enabled = !IsPaused;
            if (IsPaused)
            {
                if (IsGateMenu)
                {
                    PauseTitle = "ÁTLÉPÉS ÚJ PÁLYÁRA";
                } else if (IsShopping)
                {
                    PauseTitle = "ÜRÁLLOMÁS BOLT";
                } else if (Spaceship.Health == 0)
                {
                    PauseTitle = "AZ ÜRHAJÓ FELROBBANT, A JÁTÉKNAK VÉGE!";
                }
                else if (Spaceship.Fuel == 0)
                {
                    PauseTitle = "KIFOGYOTT AZ ÜZEMANYAG, A JÁTÉKNAK VÉGE!";
                }
                else if (RemainingSeconds == 0)
                {
                    PauseTitle = "AZ IDÖ LEJÁRT, A JÁTÉKNAK VÉGE!";
                }
                else if (CompletedLevelCount == 3)
                {
                    PauseTitle = "AZ ÖSSZES PÁLYÁT TELJESÍTETTED, GRATULÁLOK!";
                }
                else
                {
                    PauseTitle = "JÁTÉK MEGÁLLÍTVA";
                }
            } else
            {
                ShowExit = true;
                IsShopping = false;
                IsGateMenu = false;
            }
        }

        public void EndGame()
        {
            DoSaveGame = false;
            CanTogglePause = false;
            SetPause(true);
        }

        public void OpenShop()
        {
            if (CanTogglePause)
            {
                ShowExit = false;
                lockShop = true;
                IsShopping = true;
                SetPause(true);
            }
        }

        public void OpenGate()
        {
            if (CanTogglePause)
            {
                Spaceship.SpeedX = 0;
                Spaceship.SpeedY = 0;
                ShowExit = false;
                lockGate = true;
                IsGateMenu = true;
                SetPause(true);
            }
        }

        public void DisableControl(Key control, bool disable)
        {
            if (disable)
            {
                if (!disabledControls.Contains(control))
                {
                    disabledControls.Add(control);
                }
            } else
            {
                disabledControls.Remove(control);
            }
        }

        public bool HoldingKey(Key key)
        {
            return holding.Contains(key) && !disabledControls.Contains(key);
        }

        public void FreezeSpaceship(bool freeze)
        {
            DisableControl(Key.Up, freeze);
            DisableControl(Key.Down, freeze);
            DisableControl(Key.Left, freeze);
            DisableControl(Key.Right, freeze);
            Spaceship.SpeedX = 0;
            Spaceship.SpeedY = 0;
        }

        public void Restart()
        {
            CanTogglePause = true;
            SetPause(false);
        }

        internal void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!holding.Contains(e.Key)) holding.Add(e.Key);
            if (CanTogglePause)
            {
                if (e.Key == Key.Escape)
                {
                    SetPause(!IsPaused);
                }
                else if (e.Key == Key.System && e.SystemKey == Key.F4)
                {
                    if (!IsPaused)
                    {
                        e.Handled = true;
                        SetPause(true);
                        PauseTitle = "MEGMENTETTELEK EGY MEGGONDOLATLAN KILÉPÉSTÖL...SZÍVESEN.";
                    }
                }
            }
        }

        internal void OnKeyUp(object sender, KeyEventArgs e)
        {
            holding.Remove(e.Key);
        }

        public void SaveGame()
        {
            if (!DoSaveGame)
            {
                DeleteSave();
                MainWindow.MainController.CheckSaveGame();
                return;
            }

            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".starlinktaxi", "save.json");
            FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                Directory.CreateDirectory(fi.DirectoryName);
            }
            StreamWriter save = new StreamWriter(path);

            util.MissionReward reward = new util.MissionReward() { Money = CurrentLevel.CurrentMission.Reward.Money, Seconds = CurrentLevel.CurrentMission.Reward.Seconds };
            util.Mission missionData = new util.Mission() { Type = (int)CurrentLevel.CurrentMission.Type, ElementIndex = CurrentLevel.Docks.TakeWhile(element => element != CurrentLevel.CurrentMission.Element).Count(), Reward = reward };

            util.Spaceship spaceshipData = new util.Spaceship() { X = Spaceship.X, Y = Spaceship.Y, Health = Spaceship.Health, Fuel = Spaceship.Fuel };

            GameData data = new GameData() { CompletedLevelCount = CompletedLevelCount, LevelName = CurrentLevel.LevelName, RemaniningSeconds = RemainingSeconds, Money = Money, MissionData = missionData, SpaceshipData = spaceshipData };

            string jsonString = JsonSerializer.Serialize(data);

            save.Write(jsonString);
            save.Flush();
            save.Close();

            MainWindow.MainController.CheckSaveGame();
        }

        public async void InitFromSave()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".starlinktaxi", "save.json");
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                GameData data = JsonSerializer.Deserialize<GameData>(File.ReadAllText(path));

                CompletedLevelCount = data.CompletedLevelCount;
                await NextLevel();

                level.Mission mission = new level.Mission() { Element = CurrentLevel.Docks.ElementAt(data.MissionData.ElementIndex), IsCompleted = false, Type = (MissionType) data.MissionData.Type, Reward = new level.MissionReward(data.MissionData.Reward.Money, data.MissionData.Reward.Seconds) };
                CurrentLevel.OverrideMission(mission);

                Spaceship.X = data.SpaceshipData.X;
                Spaceship.Y = data.SpaceshipData.Y;
                Spaceship.Health = data.SpaceshipData.Health;
                Spaceship.Fuel = data.SpaceshipData.Fuel;

                Money = data.Money;
                RemainingSeconds = data.RemaniningSeconds;
            }
        }

        public void DeleteSave()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".starlinktaxi", "save.json");
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                fi.Delete();
            }
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            if (HoldingKey(Key.Left))
            {
                if (Spaceship.SpeedX > -10)
                {
                    Spaceship.SpeedX -= 0.05;
                }
                if (Spaceship.ScaleX != 1)
                {
                    Spaceship.X -= 100;
                    Spaceship.ScaleX = 1;
                }
            }
            else
            {
                if (Spaceship.SpeedX < 0.00)
                {
                    Spaceship.SpeedX += 0.05;
                }
            }

            if (HoldingKey(Key.Right))
            {
                if (Spaceship.SpeedX < 10)
                {
                    Spaceship.SpeedX += 0.05;
                }
                if (Spaceship.ScaleX != -1)
                {
                    Spaceship.X += 100;
                    Spaceship.ScaleX = -1;
                }
            }
            else
            {
                if (Spaceship.SpeedX > 0.00)
                {
                    Spaceship.SpeedX -= 0.05;
                }
            }

            if (HoldingKey(Key.Up))
            {
                if (Spaceship.SpeedY > -10)
                {
                    Spaceship.SpeedY -= 0.05;
                }
            }
            else
            {
                if (Spaceship.SpeedY < 0.00)
                {
                    Spaceship.SpeedY += 0.05;
                }
                Spaceship.SpeedY += 0.15 * CurrentLevel.Gravity;
            }

            if (HoldingKey(Key.Down))
            {
                if (Spaceship.SpeedY < 10)
                {
                    Spaceship.SpeedY += 0.05;
                }
            }
            else
            {
                if (Spaceship.SpeedY > 0.00)
                {
                    Spaceship.SpeedY -= 0.05;
                }
            }

            string model = null;
            if ((HoldingKey(Key.Left) || HoldingKey(Key.Right)) && !HoldingKey(Key.Up))
            {
                model = "resource/spaceship/spaceship_flame_side.png";
            }
            else if (HoldingKey(Key.Up) && !(HoldingKey(Key.Left) || HoldingKey(Key.Right))) {
                model = "resource/spaceship/spaceship_flame_under.png";
            }
            else if (HoldingKey(Key.Up) && (HoldingKey(Key.Left) || HoldingKey(Key.Right)))
            {
                model = "resource/spaceship/spaceship_flame_all.png";
            }
            else
            {
                model = "resource/spaceship/spaceship.png";
            }
            if (Spaceship.Model != model) Spaceship.Model = model;

            bool? isColliding = null;
            try
            {
                CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                {
                    CollideSide spaceshipWithBorder = CollideSide.NONE;
                    if (Spaceship.Position.X < 0)
                    {
                        spaceshipWithBorder = CollideSide.LEFT;
                        Spaceship.X = Spaceship.ScaleX == -1 ? 85 : 0;
                    }
                    else if (Spaceship.Position.X + Spaceship.SizeX > CurrentLevel.Root.Width)
                    {
                        Spaceship.X = CurrentLevel.Root.Width - Spaceship.SizeX + (Spaceship.ScaleX == -1 ? 85 : 0);
                        spaceshipWithBorder = CollideSide.RIGHT;
                    }
                    else if (Spaceship.Position.Y < 0)
                    {
                        spaceshipWithBorder = CollideSide.TOP;
                        Spaceship.Y = 0;
                    }
                    else if (Spaceship.Position.Y + Spaceship.SizeY > CurrentLevel.Root.Height)
                    {
                        Spaceship.Y = CurrentLevel.Root.Height - Spaceship.SizeY;
                        spaceshipWithBorder = CollideSide.BOTTOM;
                    }

                    if (spaceshipWithBorder != CollideSide.NONE)
                    {
                        if (spaceshipWithBorder == CollideSide.LEFT || spaceshipWithBorder == CollideSide.RIGHT)
                        {
                            if (Math.Abs(Spaceship.SpeedX) > 2.0) Spaceship.Health -= Math.Abs(Spaceship.SpeedX) * 2;
                            Spaceship.SpeedX = 0;
                        }
                        else
                        {
                            if (Math.Abs(Spaceship.SpeedY) > 2.0) Spaceship.Health -= Math.Abs(Spaceship.SpeedY) * 2;
                            Spaceship.SpeedY = 0;
                        }
                        isColliding = true;
                    }

                    foreach (LevelElement element in CurrentLevel.Elements.Concat(CurrentLevel.Docks).Concat(CurrentLevel.Shops))
                    {
                        if (element is CollidableLevelElement)
                        {
                            Point? bounds = (element as CollidableLevelElement).IsColliding(Spaceship);
                            if (bounds != null)
                            {
                                double x = ((Point)bounds).X;
                                double y = ((Point)bounds).Y;
                                Spaceship.X += x;
                                Spaceship.Y += y;

                                if (Math.Abs(x) > 0)
                                {
                                    if (Math.Abs(Spaceship.SpeedX) > 2.0) Spaceship.Health -= Math.Abs(Spaceship.SpeedX) * 2;
                                    Spaceship.SpeedX = 0;
                                }
                                else
                                {
                                    if (Math.Abs(Spaceship.SpeedY) > 2.0) Spaceship.Health -= Math.Abs(Spaceship.SpeedY) * 2;
                                    Spaceship.SpeedY = 0;
                                    // If landed on element
                                    if (y < 0)
                                    {
                                        if (!waiting && element is level.element.Dock)
                                        {
                                            level.element.Dock landedOn = element as level.element.Dock;
                                            if (landedOn == CurrentLevel.CurrentMission.Element)
                                            {
                                                waiting = true;
                                                FreezeSpaceship(true);
                                                if (CurrentLevel.CurrentMission.Type == MissionType.PICKUP)
                                                {
                                                    enterAnimation.From = Canvas.GetLeft(CurrentLevel.Spaceman.Root);
                                                    enterAnimation.To = (Spaceship.Position.X + Spaceship.SizeX / 2) - CurrentLevel.Spaceman.Root.Width / 2;
                                                    CurrentLevel.Spaceman.Root.BeginAnimation(Canvas.LeftProperty, enterAnimation);
                                                    MissionTitle = "VÁRJ, AMÍG BESZÁLL AZ UTAS";
                                                }
                                                else if (CurrentLevel.CurrentMission.Type == MissionType.TRANSPORT)
                                                {
                                                    exitAnimation.From = (Spaceship.Position.X + Spaceship.SizeX / 2) - CurrentLevel.Spaceman.Root.Width / 2;
                                                    exitAnimation.To = Canvas.GetLeft((CurrentLevel.CurrentMission.Element as LevelElement).Root) + (((CurrentLevel.CurrentMission.Element as LevelElement).Root.Width / 2) - CurrentLevel.Spaceman.Root.Width / 2);
                                                    MissionTitle = "VÁRJ, AMÍG KISZÁLL AZ UTAS";
                                                }
                                                new System.Threading.Thread(() =>
                                                {
                                                    if (CurrentLevel.CurrentMission.Type == MissionType.TRANSPORT)
                                                    {
                                                        CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            CurrentLevel.Spaceman.Spawn(CurrentLevel.CurrentMission.Element as LevelElement);
                                                            Canvas.SetLeft(CurrentLevel.Spaceman.Root, (Spaceship.Position.X + Spaceship.SizeX / 2) - CurrentLevel.Spaceman.Root.Width / 2);
                                                        }));
                                                        System.Threading.Thread.Sleep(500);
                                                        CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            CurrentLevel.Spaceman.Root.BeginAnimation(Canvas.LeftProperty, exitAnimation);
                                                        }));
                                                    }
                                                    System.Threading.Thread.Sleep(1500);
                                                    if (CurrentLevel.CurrentMission.Type == MissionType.PICKUP)
                                                    {
                                                        CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            CurrentLevel.Spaceman.Despawn();
                                                        }));
                                                        System.Threading.Thread.Sleep(500);
                                                    }
                                                    CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        CurrentLevel.CompleteMission();
                                                    }));
                                                }).Start();
                                            }
                                        }
                                        if (element is Shop)
                                        {
                                            if (!lockShop && CurrentLevel.CurrentMission.Type != MissionType.TRANSPORT)
                                            {
                                                OpenShop();
                                            }
                                        }
                                    }
                                }
                                isColliding = true;
                                break;
                            }
                            else
                            {
                                if (element is Shop && lockShop && Canvas.GetTop(element.Root) - (Spaceship.Y + Spaceship.SizeY) > 5)
                                {
                                    lockShop = false;
                                }
                            }
                        }

                        if (isColliding == null) isColliding = false;
                    }

                    Point? gateCollide = CurrentLevel.Gate.IsColliding(Spaceship);
                    if (gateCollide != null && !lockGate)
                    {
                        OpenGate();
                    }
                    else
                    {
                        if (lockGate && Spaceship.Y - (Canvas.GetTop(CurrentLevel.Gate.Root) + CurrentLevel.Gate.Root.Height) > 5)
                        {
                            lockGate = false;
                        }
                    }
                }));
            }
            catch (TaskCanceledException) { }

            while (isColliding == null)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (!(bool)isColliding)
            {
                Spaceship.X += Spaceship.SpeedX;
                Spaceship.Y += Spaceship.SpeedY;
                Spaceship.Fuel -= (1 + Math.Abs(Spaceship.SpeedX) + Math.Abs(Spaceship.SpeedY)) / 500;
            }
        }

    }
}
