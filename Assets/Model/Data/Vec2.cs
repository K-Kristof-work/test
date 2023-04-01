using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    public struct Vec2
    {
        public uint x;
        public uint y;

        public Vec2(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
