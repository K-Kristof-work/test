﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Data;

namespace Assets.Model
{
    public class PlayerAction
    {
        private GameData gameData;

        public delegate void ZoneSelectEventHandler(int id, int tx, int tz, int bx, int bz);
        public delegate void ZoneInfoEventHandler(ZoneType zoneType, int fullness, int capacity, int taxes, int lvl, int happiness);

        public event ZoneSelectEventHandler OnZoneSelected;
        public event ZoneInfoEventHandler OnZoneInfo;

        public PlayerAction (GameData gd)
        {
            gameData = gd;
        }

		public void SelectZone(int x, int z)
		{
			int id = gameData.grid[x][z].zone.zone_id;

			if (id == 0 || id == -1) return;

			int minX = x, minZ = z, maxX = x, maxZ = z;

			for (int i = 0; i < gameData.gridWidth; i++)
			{
				for (int j = 0; j < gameData.gridHeight; j++)
				{
					if (gameData.grid[i][j].zone.zone_id == id)
					{
						if (i < minX) minX = i;
						if (i > maxX) maxX = i;
						if (j < minZ) minZ = j;
						if (j > maxZ) maxZ = j;
					}
				}
			}

			OnZoneSelected?.Invoke(id, minX, minZ, maxX, maxZ);
		}

		public void UpgradeZone(int id)
		{
            gameData.GetZoneDataById(id).zone_level++;
			ZoneInfo(id);
        }

		public void IncreaseTax(int id)
		{
            gameData.GetZoneDataById(id).zone_tax++;
            ZoneInfo(id);
        }

        public void DecreaseTax(int id)
        {
            gameData.GetZoneDataById(id).zone_tax--;
            ZoneInfo(id);
        }

        public void ZoneInfo(int id)
        {
			ZoneData zd = gameData.GetZoneDataById(id);

			if(zd != null)
			{
				OnZoneInfo?.Invoke(gameData.GetZoneType(id), zd.zone_population, zd.zone_capacity, zd.zone_tax, zd.zone_level, zd.zone_happiness);
			}
        }
    }
}
