﻿using starlinktaxi.game.level;
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
            Title = "FÖLD";
            Gravity = 1.0;
            Spawnpoint = Elements.ElementAt(0);
        }
    }
}
