using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class Field
    {
        public Vec2 pos; // top left corner
        public ZoneType zoneType;
        public Block block;
        public bool IsConnectedToIncomingRoad = false;
    }
}
