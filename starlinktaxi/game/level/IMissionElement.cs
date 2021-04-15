using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starlinktaxi.game.level
{
    public interface IMissionElement
    {

    }

    public class MissionReward
    {

        public double Money { get; }
        public int Seconds { get; }

        public MissionReward(double money, int seconds)
        {
            Money = money;
            Seconds = seconds;
        }

    }


    public enum MissionType {

        PICKUP, TRANSPORT

    }

    public class Mission
    {

        public IMissionElement Element { get; set; }
        public MissionType Type { get; set; }
        public MissionReward Reward { get; set; }
        public bool IsCompleted { get; set; } = false;

    }
}
