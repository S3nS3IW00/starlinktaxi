using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starlinktaxi.util
{
    public class GameData
    {
        public Spaceship SpaceshipData { get; set; }
        public string LevelName { get; set; }
        public Mission MissionData { get; set; }
        public int CompletedLevelCount { get; set; }
        public int RemaniningSeconds { get; set; }
        public double Money { get; set; }
    }

    public class Spaceship
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Health { get; set; }
        public double Fuel { get; set; }
    }

    public class MissionReward
    {
        public double Money { get; set; }
        public int Seconds { get; set; }
    }

    public class Mission
    {
        public int Type { get; set; }
        public int ElementIndex { get; set; }
        public MissionReward Reward { get; set; }
    }
}
