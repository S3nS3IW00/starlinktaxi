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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace starlinktaxi.game
{
    public class GameHandler : Bindable
    {

        private readonly List<Key> holding = new List<Key>();
        private readonly List<Key> disabledControls = new List<Key>();
        public Timer Timer { get; }
        private Timer remainingSecondTimer;
        private bool waiting = false;

        private bool loading = false;
        public bool IsLoading { get => loading; set { loading = value; ControlPropertyChanged(); } }

        private Cursor cursor;
        private bool paused = false, canTogglePause = true;
        private string pauseTitle;

        public bool IsPaused { get => paused; set { paused = value; ControlPropertyChanged(); } }
        public bool CanTogglePause { get => canTogglePause; set { canTogglePause = value; ControlPropertyChanged(); } }
        public Cursor Cursor { get => cursor; set { cursor = value; ControlPropertyChanged(); } }
        public string PauseTitle { get => pauseTitle; set { pauseTitle = value; ControlPropertyChanged(); } }

        public Spaceship Spaceship { get; set; } = new Spaceship() { X = 0, Y = 0, ScaleX = -1, Model = "resource/spaceship.png" };

        private Level currentLevel;
        public Level CurrentLevel { get => currentLevel; set { currentLevel = value; ControlPropertyChanged(); } }

        private double money = 0;
        private int remainingSeconds = 60;
        private string missionTitle;
        public double Money { get => money; set { money = value; ControlPropertyChanged(); } }
        public int RemainingSeconds { get => remainingSeconds; set { remainingSeconds = value; ControlPropertyChanged(); } }
        public string MissionTitle { get => missionTitle; set { missionTitle = value; ControlPropertyChanged(); } }

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

            remainingSecondTimer = new Timer() { Interval = 1000, Enabled = true };
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
            LoadLevel(new Level1());
            Timer = new Timer() { Interval = 1, Enabled = true };
            Timer.Elapsed += OnTick;
        }

        public void LoadLevel(Level level)
        {
            CurrentLevel = level;
            Spaceship.Spawn(level.Spawnpoint);

            CurrentLevel.NewMission += (mission) =>
            {
                if (mission == null)
                {
                    // next level
                    EndGame();
                }
                else
                {
                    if (mission.Type == MissionType.PICKUP)
                    {
                        MissionTitle = "MENJ AZ UTASHOZ";
                    }
                    else if (mission.Type == MissionType.TRANSPORT)
                    {
                        MissionTitle = "MENJ A KIJELÖLT HELYRE";
                    }
                    else if (mission.Type == MissionType.GATE)
                    {
                        MissionTitle = "MENJ A KAPUHOZ";
                    }
                    FreezeSpaceship(false);
                    waiting = false;
                }
            };

            CurrentLevel.MissionCompleted += (reward) =>
            {
                Money += reward.Money;
                RemainingSeconds += reward.Seconds;
                MissionTitle = "JUTALOM: " + reward.Seconds + " MÁSODPERC ÉS $" + reward.Money;
            };

            CurrentLevel.Start();
        }

        public void SetPause(bool pause)
        {
            IsPaused = pause;
            Timer.Enabled = !pause;
            remainingSecondTimer.Enabled = !IsPaused;
            if (IsPaused)
            {
                if(CurrentLevel.CurrentMission == null)
                {
                    PauseTitle = "GRATULÁLOK, PÁLYA TELJESÍTVE! ÚJ PÁLYA HAMAROSAN...";
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
                else
                {
                    PauseTitle = "JÁTÉK MEGÁLLÍTVA";
                }
            }
        }

        public void EndGame()
        {
            CanTogglePause = false;
            SetPause(true);
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
                Spaceship.SpeedY += CurrentLevel.Gravity;
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

            if (HoldingKey(Key.Down) || HoldingKey(Key.Up) || HoldingKey(Key.Left) || HoldingKey(Key.Right))
            {
                Spaceship.Model = "resource/spaceship_flame.png";
            }
            else
            {
                Spaceship.Model = "resource/spaceship.png";
            }

            bool? isColliding = null;
            try
            {
                CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                {
                    CollideSide spaceshipWithBorder = CollideSide.NONE;
                    if (Spaceship.Position.X < 0)
                    {
                        spaceshipWithBorder = CollideSide.LEFT;
                        Spaceship.X = Spaceship.ScaleX == -1 ? 100 : 0;
                    }
                    else if (Spaceship.Position.X + Spaceship.SizeX > CurrentLevel.Root.Width)
                    {
                        Spaceship.X = CurrentLevel.Root.Width - Spaceship.SizeX + (Spaceship.ScaleX == -1 ? 100 : 0);
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

                    foreach (LevelElement element in CurrentLevel.Elements.Concat(CurrentLevel.Docks))
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
                                    if (y < 0)
                                    {
                                        if (!waiting && element is Dock)
                                        {
                                            Dock landedOn = element as Dock;
                                            if (landedOn == CurrentLevel.CurrentMission.Element)
                                            {
                                                waiting = true;
                                                FreezeSpaceship(true);
                                                if (CurrentLevel.CurrentMission.Type == MissionType.PICKUP)
                                                {
                                                    MissionTitle = "VÁRJ, AMÍG BESZÁLL AZ UTAS";
                                                } else if(CurrentLevel.CurrentMission.Type == MissionType.TRANSPORT)
                                                {
                                                    MissionTitle = "VÁRJ, AMÍG KISZÁLL AZ UTAS";
                                                }
                                                new System.Threading.Thread(() =>
                                                {
                                                    System.Threading.Thread.Sleep(2000);
                                                    CurrentLevel.Root.Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        CurrentLevel.CompleteMission();
                                                    }));
                                                }).Start();
                                            }
                                        }
                                    }
                                }
                                isColliding = true;
                                break;
                            }
                        }
                    }

                    Point? gateCollide = CurrentLevel.Gate.IsColliding(Spaceship);
                    if (gateCollide != null && CurrentLevel.CurrentMission.Type == MissionType.GATE)
                    {
                        CurrentLevel.CompleteMission();
                    }

                    if (isColliding == null) isColliding = false;
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
                Spaceship.Fuel -= (1 + Math.Abs(Spaceship.SpeedX) + Math.Abs(Spaceship.SpeedY)) / 1000;
            }
        }

    }
}
