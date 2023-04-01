using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    abstract class Field
    {
        public Vec2 pos; // top left corner
        public Vec2 size; // pos + (x, y)
        public int direction; // 0-3 * 90 degrees 

        public static bool IsOverlap(Field a, Field b)
        {
            Vec2 ra = new Vec2(a.pos.x + a.size.x, a.pos.y - a.size.y);
            Vec2 rb = new Vec2(b.pos.x + b.size.x, b.pos.y - b.size.y);

            if (a.pos.x > rb.x || b.pos.x > ra.x)
                return false;

            if (ra.y > b.pos.y || rb.y > a.pos.y)
                return false;

            return true;
        }
    }
}
