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

        public Vec2 getCenterPoz()
        {
            return new Vec2((pos.x + block.blockSize.x)/2, (pos.y + block.blockSize.y)/2);
        }

        public static double distanceFrom2Field(Field one, Field two)
        {
            Vec2 oneCenter = one.getCenterPoz();
            Vec2 twoCenter = two.getCenterPoz();
            return Math.Sqrt(Math.Pow(oneCenter.x - twoCenter.x, 2) + Math.Pow(oneCenter.y - twoCenter.y, 2));
        }
    }
}
