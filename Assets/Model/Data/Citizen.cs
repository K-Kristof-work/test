using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    public class Citizen
    {
        public double salary;
        public double paidTaxes;
        public int age;
        public double happiness;
        public bool diploma;
        public bool bsc;
        public double highSchoolEducation;
        public double universityEducation;
        public Field home;
        public Field work;

        public Citizen()
        {
            salary = 0;
            paidTaxes = 0;
            age = 0;
            happiness = 0;
            home = null;
            work = null;
            highSchoolEducation = 0;
            universityEducation = 0;
            diploma = false;
            bsc = false;

        }

        public Citizen(int age)
        {
            salary = 0;
            paidTaxes = 0;
            this.age = age;
            happiness = 0;
            home = null;
            work = null;
            highSchoolEducation = 0;
            universityEducation = 0;
            diploma = false;
            bsc = false;
        }
    }
}
