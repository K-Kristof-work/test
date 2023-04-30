using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class Time
    {
        public DateTime date;
        public int speed;

        // TODO add functions
        public int getSeason()
        {
            if (date.Month >= 3 && date.Month <= 5)
            {
                return 0;
            }
            else if (date.Month >= 6 && date.Month <= 8)
            {
                return 1;
            }
            else if (date.Month >= 9 && date.Month <= 11)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}
