using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class GameData
    {
        public int balance;
        public int[] taxes;
        public int loans;
        public List<Block> board;
        public int boardSize;
        public List<Citizen> citizens;
        public Time time;

        public List<Block> getBuildings()
        {
            return board.FindAll(b => GameConfig.Buildings.Contains(b.type));
        }

        public int getOperatingCost()
        {
            return getBuildings().AsEnumerable().Sum(b => b.operating_cost);
        }

        public int getTotalLevel()
        {
            return getBuildings().AsEnumerable().Sum(b => b.lvl);
        }

        public List<Block> findBlocks(BlockType bt)
        {
            return board.FindAll(b => b.type == bt);
        }

        public List<Block> getRoads()
        {
            return findBlocks(BlockType.Road);
        }

        public List<Block> getPoliceStation()
        {
            return findBlocks(BlockType.PoliceStation);
        }

        public List<Block> getPowerPlant()
        {
            return findBlocks(BlockType.PowerPlant);
        }

        public List<Block> getPowerLine()
        {
            return findBlocks(BlockType.PowerLine);
        }

        public List<Block> getStadium()
        {
            return findBlocks(BlockType.Stadium);
        }

        public List<Block> getSchool()
        {
            return findBlocks(BlockType.School);
        }

        public List<Block> getUniversity()
        {
            return findBlocks(BlockType.University);
        }

        public List<Block> getForest()
        {
            return findBlocks(BlockType.Forest);
        }
    }
}
