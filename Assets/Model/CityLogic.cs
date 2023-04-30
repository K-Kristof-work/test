using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Data;

namespace Assets.Model
{
    class CityLogic
    {
        private double industryRadius = 2.0;
        private double forestRadius = 2.0;
        private double homeToWorkRadius = 1.0;
        private double highSchoolRadius = 2.0;
        private double universityRadius = 2.0;
        private double standardCitizenHapiness = 1.0;

        private double powerConnectivityRadius = 2.0;


        private double safetyRadius = 2.0;

        private int standardResidenceTax = 5;
        private int standardCommercialTax = 5;
        private int standardIndustrialTax = 5;

        private int season = 0;

        private GameData data;
        public CityLogic (GameData gd)
        {
            data = gd;
        }

        public void Update()
        {
            UpdateTime();
            //Every quarter of a year, get taxes, pay pension and maintenance costs
            if (season != data.time.getSeason())
            {
                GetTaxes();
                PayPension();
                PayMaintenanceCosts();
                //if its not winter, grow the forests
                if (data.time.getSeason() != 3)
                {
                    GrowForests();
                }
            }
            //Every year, update citizen age and kill the elderly
            if (season == 3 && data.time.getSeason() == 0) { 
                UpdateCitizenAge();
                KillTheElderly();

            }
            EducateCitizens();
            UpdateCitizenHappiness();
            UpdatePowerConnectivity();

            season = data.time.getSeason();
        }

        private void UpdateTime()
        {
            //If the game is paused, don't update the time
            if(data.time.speed == 0)
            {
                return;
            }
            //If the game is not paused, update the time
            else
            {
                data.time.date = data.time.date.AddMinutes(data.time.speed);
            }
        }

        private void UpdateCitizenAge()
        {
            foreach (Citizen citizen in data.citizens)
            {
                citizen.age++;
            }
        }

        private void GetTaxes()
        {
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age < 65)
                {
                    data.balance += (int)(citizen.salary * (data.residencialTax * 0.01));
                    citizen.paidTaxes += (int)(citizen.salary * (data.residencialTax * 0.01));
                }
            }
        }

        private void PayPension()
        {
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age >= 65)
                {
                    data.balance -= (int)(citizen.paidTaxes / 2.0);
                }
            }            
        }

        private void PayMaintenanceCosts()
        {
            foreach (List<Field> row in data.grid)
            {
                foreach (Field field in row)
                {
                    if (field.block.type != BlockType.Empty)
                    {
                        data.balance -= field.block.operating_cost;
                    }
                }
            }
        }

        private void GrowForests()
        {
            foreach (List<Field> row in data.grid)
            {
                foreach (Field field in row)
                {
                    if (field.block.type == BlockType.Forest && field.block.lvl < 3)
                    {
                        field.block.building_progress++;
                        if (field.block.building_progress == 10)
                        {
                            field.block.lvl++;
                            field.block.building_progress = 0;
                        }
                    }
                }
            }
        }

        private void UpdateCitizenHappiness()
        {
            foreach (Citizen citizen in data.citizens)
            {
                double safety = 0.0;
                citizen.happiness = standardCitizenHapiness;

                //if a citizen works close to home, increase happiness
                if (Field.distanceFrom2Field(citizen.home, citizen.work) <= homeToWorkRadius)
                {
                    citizen.happiness += 0.01;
                }
                //if taxes are more then double the standard, decrease happiness
                if (data.residencialTax > standardResidenceTax * 2 || data.commercialTax > standardCommercialTax * 2 || data.industrialTax > standardIndustrialTax * 2)
                {
                    citizen.happiness -= 0.01;
                }
                //if taxex are less then 1.5 than the standard, increase happiness
                else if (data.residencialTax < standardResidenceTax * 1.5 || data.commercialTax < standardCommercialTax * 1.5 || data.industrialTax < standardIndustrialTax * 1.5)
                {
                    citizen.happiness += 0.01;
                }
                foreach (List<Field> row in data.grid)
                {
                    foreach (Field field in row)
                    {
                        //if factory is near citizen, decrease happiness
                        if (field.block.type == BlockType.Factory && Field.distanceFrom2Field(citizen.home, field) <= industryRadius)
                        {
                            citizen.happiness -= 0.01;
                        }
                        //if a forest is near citizen, increase happiness
                        if (field.block.type == BlockType.Forest && Field.distanceFrom2Field(citizen.home, field) <= forestRadius)
                        {
                            //depending on how big the forest is, increase happiness
                            citizen.happiness += 0.01 * (field.block.lvl + 1) + (0.001 * field.block.building_progress);
                        }
                        //Increase safety depending on how close the police station is
                        if (field.block.type == BlockType.PoliceStation)
                        {
                            citizen.happiness += 0.01;
                            if(Field.distanceFrom2Field(citizen.home, field) <= safetyRadius/2)
                            {
                                safety += 0.01;
                            }
                            else if (Field.distanceFrom2Field(citizen.home, field) <= safetyRadius)
                            {
                                safety += 0.005;
                            }
                            else if (Field.distanceFrom2Field(citizen.home, field) <= safetyRadius*2)
                            {
                                safety += 0.0025;
                            }
                        }
                    }
                }
                //increase happiness by the safety
                citizen.happiness += safety;
                //if the balance is negative, decrease happiness depending on how negative it is
                if (data.balance < 0)
                {
                    citizen.happiness -= data.balance / 100000;
                }
                //count the number of factories and commercial buildings
                int factories = 0;
                int commercials = 0;
                foreach (List<Field> row in data.grid)
                {
                    foreach (Field field in row)
                    {
                        if (field.block.type == BlockType.Factory)
                        {
                            factories++;
                        }
                        else if (field.block.type == BlockType.Shop)
                        {
                            commercials++;
                        }
                    }
                }
                //if the ratio of factories to commercials is not close to 1, decrease happiness
                if (factories / commercials > 1.2 || factories / commercials < 0.8)
                {
                    citizen.happiness -= 0.01;
                }
            }
        }

        private void EducateCitizens()
        {
            foreach (Citizen citizen in data.citizens)
            {
                foreach (List<Field> row in data.grid)
                {
                    foreach (Field field in row)
                    {
                        //if a school is near a citizen, increase education
                        if (field.block.type == BlockType.School && Field.distanceFrom2Field(citizen.home, field) <= highSchoolRadius)
                        {
                            citizen.highSchoolEducation += 0.01;
                            if (citizen.highSchoolEducation >= 10)
                            {
                                citizen.diploma = true;
                            }
                        }
                        //if a university is near a citizen, increase education
                        if (field.block.type == BlockType.University && Field.distanceFrom2Field(citizen.home, field) <= universityRadius)
                        {
                            if (citizen.diploma)
                            {
                                citizen.universityEducation += 0.01;
                                if (citizen.universityEducation >= 20)
                                {
                                    citizen.bsc = true;
                                }
                            }                            
                        }
                    }
                }
            }
        }

        private void KillTheElderly()
        {
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age >= 65)
                {
                    //Theres a 1 in 1000 chance that a citizen dies
                    Random rnd = new Random();
                    if (rnd.Next(0, 1000) == 0)
                    {
                        data.citizens.Remove(citizen);
                        data.citizens.Add(new Citizen(18));
                        //TODO: The new citizen needs to find a home and a job
                    }
                }
            }
        }

        private void UpdatePowerConnectivity()
        {

        }
    }
}
