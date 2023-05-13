using System;
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
        public delegate void ZoneInfoEventHandler(ZoneType zoneType);

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

            // Find top left buttom right corners
            int tx = -1, tz = -1, bx = -1, bz = -1;

            for(int i = 0; i < gameData.gridWidth; i++)
            {
                for (int j = 0; j < gameData.gridHeight; j++)
                {
                    if(gameData.grid[i][j].zone.zone_id == id)
                    {
                        bx = i;
                        bz = j;

                        if (tx == -1)
                        {
                            tx = i;
                            tz = j;
                        }
                    }                   
                }
            }

            OnZoneSelected?.Invoke(id, tx, tz, bx, bz);
        }

        public void ZoneInfo(int id)
        {
            OnZoneInfo?.Invoke(gameData.GetZoneType(id));
        }
    }
}
