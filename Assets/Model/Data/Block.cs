using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    public class Block
    {
        public BlockType type;
        public int lvl; // 0-3, 0 = building in progress
        public int building_progress; // 1-10
        public int operating_cost;
        public bool isPowered;
        public double powerConsumption;
        public int building_cost;
        public List<Citizen> citizens = new();
        public Vec2 blockSize;
        public Vec2 midPosition;

        public void PowerConsumption()
        {

        }

        public void setDefaultValues()
        {
            //create cases for each type of building

            switch (type)
            {
                case BlockType.Empty:
                    lvl = 0;
                    operating_cost = 0;
                    isPowered = false;
                    powerConsumption = 0;
                    building_cost = 0;
                    break;

                case BlockType.Road:
                    lvl = 0;
                    operating_cost = 5;
                    isPowered = false;
                    powerConsumption = 0;
                    building_cost = 100;
                    break;

                case BlockType.House:
                    lvl = 1;
                    operating_cost = 10;
                    isPowered = false;
                    powerConsumption = 5;
                    building_cost = 0;
                    break;

                case BlockType.Shop:
                    lvl = 1;
                    operating_cost = 15;
                    isPowered = false;
                    powerConsumption = 8;
                    building_cost = 0;
                    break;

                case BlockType.Factory:
                    lvl = 1;
                    operating_cost = 20;
                    isPowered = false;
                    powerConsumption = 15;
                    building_cost = 0;
                    break;

                case BlockType.PoliceStation:
                    lvl = 1;
                    operating_cost = 18;
                    isPowered = false;
                    powerConsumption = 10;
                    building_cost = 2000;
                    break;

                case BlockType.Stadium:
                    lvl = 1;
                    operating_cost = 25;
                    isPowered = false;
                    powerConsumption = 20;
                    building_cost = 5000;
                    break;

                case BlockType.School:
                    lvl = 1;
                    operating_cost = 12;
                    isPowered = false;
                    powerConsumption = 7;
                    building_cost = 1000;
                    break;

                case BlockType.University:
                    lvl = 1;
                    operating_cost = 30;
                    isPowered = false;
                    powerConsumption = 25;
                    building_cost = 3000;
                    break;

                case BlockType.Forest:
                    lvl = 0;
                    operating_cost = 0;
                    isPowered = false;
                    powerConsumption = 0;
                    building_cost = 500;
                    break;

                case BlockType.PowerPlant:
                    lvl = 1;
                    operating_cost = 50;
                    isPowered = false;
                    powerConsumption = 0;
                    building_cost = 10000;
                    break;

                case BlockType.PowerLine:
                    lvl = 0;
                    operating_cost = 0;
                    isPowered = false;
                    powerConsumption = 0;
                    building_cost = 200;
                    break;
            }
        }
    }
}
