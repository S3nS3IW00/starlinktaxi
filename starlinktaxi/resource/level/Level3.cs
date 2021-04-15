using starlinktaxi.game.level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starlinktaxi.resource.level
{
    class Level3 : Level
    {

        public Level3() : base("level3")
        {
            Title = "MARS";
            Gravity = 0.7;
            Spawnpoint = Elements.ElementAt(0);
        }

    }
}
