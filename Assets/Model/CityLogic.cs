using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Model.Data;
using System.Timers;
using static Assets.Model.Data.GameData;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Model
{
    public class CityLogic
    {
        private double industryRadius = 2.0;
        private double forestRadius = 2.0;
        private double homeToWorkRadius = 100000.0;
        private double highSchoolRadius = 2.0;
        private double universityRadius = 2.0;
        private double standardCitizenHapiness = 1.0;

        private double powerConnectivityRadius = 2.0;

        private double safetyRadius = 2.0;

        private int season = -1;

        private double happinessFromLowCommute = 0;
        private double happinessFromTax = 0;
        private double happinessFromIndustry = 0;
        private double happinessFromForest = 0;
        private double happinessFromSafety = 0;
        private double happinessFromDebt = 0;
        private double happinessFromWorkRatio = 0;

        private double happinessReward = 1;

        private int bekoltozesELoszor = 500;

        public delegate void TimeEventHandler(Assets.Model.Data.Time time);
        public delegate void MoneyEventHandler(int balance, int difference, string type);
        //public delegate void IncomeSpendingEventHandler(int money);
        //public delegate void HappinessEventHandler(double commute, double tax, double industry, double forest, double safety, double debt, double ratio);
        public delegate void OnHappinessChangedEventHandler(double happiness);
        public delegate void OnCitizenAddedEventHandler(int count);

        public event TimeEventHandler OnTimeChanged;
        public event MoneyEventHandler OnMoneyChanged;
        //public event IncomeSpendingEventHandler OnIncome;
        //public event HappinessEventHandler OnHappinessChanged;
        public event OnHappinessChangedEventHandler OnHappinessChanged;
        public event OnCitizenAddedEventHandler OnCitizenChanged;

        private Timer timer;
        private Timer seasonTimer;

        private GameData data;
        public CityLogic(GameData gd)
        {
            data = gd;
            timer = new Timer(1000);
            timer.Elapsed += Update;
            timer.AutoReset = true;
            timer.Enabled = true;

            seasonTimer = new Timer(50);
            seasonTimer.Elapsed += SeasonUpdate;
            seasonTimer.AutoReset = true;
            seasonTimer.Enabled = true;

            //Init();
            //No idea how to call the first update of the balance
        }

        /*public void Init()
        {
            //Bug in UI, first element doest show up, this fixed this and the first update of the balance aswelll
            OnMoneyChanged?.Invoke(data.balance, 0, "Init");
        }*/


        public void SeasonUpdate(object source, ElapsedEventArgs e)
        {
            UpdateTime();
            OnTimeChanged?.Invoke(data.time);

            if (season == -1)
            {
                season = data.time.getSeason();
            }
            if (season != data.time.getSeason())
            {
                data.DebugInUnity(this, "SeasonChange");
                data.DebugInUnity(this, "1");
                GetTaxes();
                data.DebugInUnity(this, "2");
                PayPension();
                data.DebugInUnity(this, "3");
                PayMaintenanceCosts();
                data.DebugInUnity(this, "4");
                //if its not winter, grow the forests
                if (data.time.getSeason() != 3)
                {
                    GrowForests();
                    data.DebugInUnity(this, "5");
                }
                if (season == 3 && data.time.getSeason() == 0)
                {
                    UpdateCitizenAge();
                    data.DebugInUnity(this, "6");
                    KillTheElderly();
                    data.DebugInUnity(this, "7");
                }

                data.DebugInUnity(this, "before " + season);

                season = data.time.getSeason();

                data.DebugInUnity(this, "after " + season);

                data.DebugInUnity(this, "8");

            }
        }

        public void Update(object source, ElapsedEventArgs e)
        {
            /*data.DebugInUnity(this, "FirstSeason " + season + "DataSeason: " + data.time.getSeason());

            data.DebugInUnity(this, "SecondSeasonOne" + season);
            season = data.time.getSeason();
            data.DebugInUnity(this, "SecondSeasonTwo" + season);*/

            EducateCitizens();
            UpdateCitizenHappiness();
            UpdatePowerConnectivity();
            data.DebugInUnity(this, "9");
            UpdateCitizens();
            data.DebugInUnity(this, "10");
            
        }

        public void ExitTimeEvent()
        {
            timer.Stop();
            timer.Dispose();
            seasonTimer.Stop();
            seasonTimer.Dispose();
        }

        private void UpdateCitizens()
        {
            //if the happiness is above 30% a new citizen will move in
            Citizen citizen = new Citizen();
            //the citizen will take a random home
            foreach (List<Field> row in data.grid)
            {
                foreach (Field field in row)
                {
                    if(field.block == null)
                    {
                        continue;
                    }
                    data.DebugInUnity(this, "11");
                    Block block = field.block;
                    data.DebugInUnity(this, "12");
                    data.DebugInUnity(this, "block.citizen.Count" + block.citizens.Count);
                    data.DebugInUnity(this, "block.lvl" + block.lvl);
                    if (block.citizens.Count <= block.lvl * 10 && block.type == BlockType.House)
                    {
                        data.DebugInUnity(this, "13");
                        citizen.home = field;
                        block.citizens.Add(citizen);
                        data.citizens.Add(citizen);
                        data.DebugInUnity(this, "Citizen moved in");

                        FindAJob(citizen);

                        OnCitizenChanged?.Invoke(data.citizens.Count);
                        data.DebugInUnity(this, "14");
                        return;
                    }
                }
            }


        }

        private void FindAJob(Citizen citizen)
        {
            foreach (List<Field> row in data.grid)
            {
                foreach (Field field in row)
                {
                    //If the field has an industry or shop building on it and it is close enough to the home of the citizen

                    if (field.block == null)
                    {
                        continue;
                    }
                    data.DebugInUnity(this, "15");
                    data.DebugInUnity(this, "fild blocktype" + field.block.type.ToString());
                    data.DebugInUnity(this, "Field.distanceFrom2Field(citizen.home, field)" + Field.distanceFrom2Field(citizen.home, field));
                    if ((field.block.type == BlockType.Factory || field.block.type == BlockType.Shop) && Field.distanceFrom2Field(citizen.home, field) <= homeToWorkRadius)
                    {
                        data.DebugInUnity(this, "16");
                        citizen.work = field;
                        data.DebugInUnity(this, "17");
                        field.block.citizens.Add(citizen);
                        data.DebugInUnity(this, "18");
                        data.DebugInUnity(this, "Citizen found a job");

                        SetSalary(citizen, field);
                        return;
                    }
                }
            }
        }

        private void SetSalary(Citizen citizen, Field field)
        {
            int degreeMultiplier = 1;
            //if the citizen has a bsc degree
            if (citizen.diploma)
            {
                degreeMultiplier = 5;
            }else if (citizen.bsc)
            {
                degreeMultiplier = 3;
            }
            //depending on the level of the building, the salary will be higher
            citizen.salary = field.block.lvl * 100 * degreeMultiplier;
            data.DebugInUnity(this, "19");
            data.DebugInUnity(this, "citizen salary was set" + citizen.salary);
        }

        private void UpdateTime()
        {
            //If the game is paused, don't update the time
            if (data.time.speed == 0)
            {
                return;
            }
            //If the game is not paused, update the time
            else
            {
                data.time.date = data.time.date.AddMinutes(data.time.speed * 20);
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
            int taxes = 0;
            data.DebugInUnity(this, "GetTaxes");
            data.DebugInUnity(this, "num of citizen" + data.citizens.Count);
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age < 65)
                {
                    data.balance += (int)(citizen.salary * citizen.work.zone.zone_tax * 0.01);
                    citizen.paidTaxes = (int)(citizen.salary * citizen.work.zone.zone_tax * 0.01);
                }
            }
            //jsut for testing
            //data.DebugInUnity(this, "wuddaheeeeeeeeellomg");
            //taxes = 1000;
            OnMoneyChanged?.Invoke(data.balance, taxes, "Tax");
        }

        private void PayPension()
        {
            int pensionCosts = 0;
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age >= 65)
                {
                    pensionCosts += (int)(citizen.salary * 0.5);
                    data.balance -= (int)(citizen.paidTaxes * 0.5);
                }
            }
            OnMoneyChanged?.Invoke(data.balance, pensionCosts, "Pension");
        }

        private void PayMaintenanceCosts()
        {
            int maintenanceCosts = 0;
            data.DebugInUnity(this, "Maintenance");
            data.DebugInUnity(this, "num of buildings" + data.GetBuildings().Count);
            foreach (Block block in data.GetBuildings())
            {
                maintenanceCosts += block.operating_cost;
            }
            data.balance -= maintenanceCosts;
            OnMoneyChanged?.Invoke(data.balance, maintenanceCosts, "MaintinenceCosts");
        }

        private void GrowForests()
        {
            data.DebugInUnity(this, "building count" + data.GetBuildings().Count.ToString());
            foreach (Block block in data.GetBuildings())
            {
                data.DebugInUnity(this, "blocktype" + block.type.ToString());
                data.DebugInUnity(this, "uno");
                if (block.type == BlockType.Forest && block.lvl < 3)
                {
                    data.DebugInUnity(this, "dos");
                    block.building_progress++;
                    if (block.building_progress == 10)
                    {
                        block.lvl++;
                        block.building_progress = 0;
                    }
                }
                data.DebugInUnity(this, "tres");
            }

            data.DebugInUnity(this, "growing forests");
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
                    happinessFromLowCommute += happinessReward;
                    citizen.happiness += happinessReward;
                }
                //if taxes are more then double the standard, decrease happiness
                //if (data.residencialTax > standardResidenceTax * 2 || data.commercialTax > standardCommercialTax * 2 || data.industrialTax > standardIndustrialTax * 2)
                /*{
                    happinessFromTax -= 0.01;
                    citizen.happiness -= 0.01;
                }*/
                //if taxex are less then 1.5 than the standard, increase happiness
                //else if (data.residencialTax < standardResidenceTax * 1.5 || data.commercialTax < standardCommercialTax * 1.5 || data.industrialTax < standardIndustrialTax * 1.5)
                /*{
                    happinessFromTax += 0.01;
                    citizen.happiness += 0.01;
                }*/
                foreach (List<Field> row in data.grid)
                {
                    foreach (Field field in row)
                    {
                        //if factory is near citizen, decrease happiness
                        if (field.block.type == BlockType.Factory && Field.distanceFrom2Field(citizen.home, field) <= industryRadius)
                        {
                            happinessFromIndustry -= happinessReward;
                            citizen.happiness -= happinessReward;
                        }
                        //if a forest is near citizen, increase happiness
                        if (field.block.type == BlockType.Forest && Field.distanceFrom2Field(citizen.home, field) <= forestRadius)
                        {
                            //depending on how big the forest is, increase happiness
                            happinessFromForest += happinessReward * (field.block.lvl + 1) + (0.1 * happinessReward * field.block.building_progress);
                            citizen.happiness += happinessReward * (field.block.lvl + 1) + (0.1 * happinessReward * field.block.building_progress);
                        }
                        //Increase safety depending on how close the police station is
                        if (field.block.type == BlockType.PoliceStation)
                        {
                            citizen.happiness += happinessReward;
                            if (Field.distanceFrom2Field(citizen.home, field) <= safetyRadius / 2)
                            {
                                safety += happinessReward;
                            }
                            else if (Field.distanceFrom2Field(citizen.home, field) <= safetyRadius)
                            {
                                safety += happinessReward / 2;
                            }
                            else if (Field.distanceFrom2Field(citizen.home, field) <= safetyRadius * 2)
                            {
                                safety += happinessReward / 4;
                            }
                        }
                    }
                }
                //increase happiness by the safety
                happinessFromSafety += safety;
                citizen.happiness += safety;
                //if the balance is negative, decrease happiness depending on how negative it is
                if (data.balance < 0)
                {
                    happinessFromDebt -= data.balance / 100000;
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
                    happinessFromWorkRatio -= happinessReward;
                    citizen.happiness -= happinessReward;
                }
                //data.DebugInUnity(this, "citizen happiness: " + citizen.happiness);
            }

            double avgHappiness = 0;
            foreach (Citizen citizen in data.citizens)
            {
                //calculate the average happiness
                avgHappiness += citizen.happiness;
            }
            avgHappiness /= data.citizens.Count;
            //use the sigmoid function to calculate the happiness
            data.happiness = 1 / (1 + Math.Exp(-avgHappiness));
            OnHappinessChanged?.Invoke(1 / (1 + Math.Exp(-avgHappiness)));
        }

        private void EducateCitizens()
        {
            foreach (Citizen citizen in data.citizens)
            {
                foreach (Block block in data.GetBuildings())
                {
                    //if a school is near a citizen, increase education
                    if (block.type == BlockType.School && Field.distanceFromFieldAndBlock(citizen.home, block.midPosition) <= highSchoolRadius)
                    {
                        citizen.highSchoolEducation += 0.01;
                        if (citizen.highSchoolEducation >= 10)
                        {
                            citizen.diploma = true;
                        }
                    }
                    //if a university is near a citizen, increase education
                    if (block.type == BlockType.University && Field.distanceFromFieldAndBlock(citizen.home, block.midPosition) <= universityRadius)
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

        private void KillTheElderly()
        {
            foreach (Citizen citizen in data.citizens)
            {
                if (citizen.age >= 65)
                {
                    //Theres a 1 in 1000 chance that a citizen dies
                    System.Random rnd = new System.Random();
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

        public void BuildingPlacedByUser(Block block)
        {
            data.balance -= block.building_cost;
            OnMoneyChanged?.Invoke(data.balance, block.building_cost, block.type.ToString());
        }

        public void SpeedChange()
        {
            if (data.time.speed == 1)
            {
                data.time.speed = 2;
            }
            else if (data.time.speed == 2)
            {
                data.time.speed = 3;
            }
            else if (data.time.speed == 3)
            {
                data.time.speed = 0;
            }
            else if (data.time.speed == 0)
            {
                data.time.speed = 1;
            }
        }

        public int SetZoneTimerInterval(ZoneData zone)
        { 
            return 1000;
            //the logic for the timer interval
        }

	}
}
