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
    class Level2 : Level
    {

        public Level2() : base("level2")
        {
            Title = "HOLD";
            Gravity = 0.5;
            Spawnpoint = Elements.ElementAt(2);
        }

    }
}
