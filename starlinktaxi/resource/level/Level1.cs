using starlinktaxi.game.level;
using starlinktaxi.game.level.element;
using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starlinktaxi.resource.level
{
    class Level1 : Level
    {
        public Level1() : base("level1")
        {
            Title = "Hello World";
            MissionCount = 5;
            Gravity = 0.1;
            do
            {
                Spawnpoint = Elements.ElementAt(GameUtil.Random.Next(Elements.Count));
            } while (!(Spawnpoint is CollidableLevelElement || Spawnpoint is Dock));
        }
    }
}
