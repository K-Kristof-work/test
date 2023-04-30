﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class Block
    {
        public BlockType type;
        public int lvl; // 0-3, 0 = building in progress
        public int building_progress; // 1-10
        public int operating_cost;
        public bool isPowered;
        public double powerConsumption;
        public Citizen[] citizens;
        public Vec2 blockSize;

        public void PowerConsumption()
        {

        }
    }
}
