using Assets.Model.Data;
using System.Collections;
using System.Collections.Generic;

public class ZoneData
{
    public int zone_id;
    public string zone_name;
    public ZoneType zone_type;
    public int zone_level;
    public int zone_population;
    public int zone_employment;
    public int zone_happiness;
    public int zone_capacity;
    public int zone_tax;
    public List<Block> zone_buildings;

    //make a constructor that takes in a all the values and sets all the values
    public ZoneData(int zone_id, ZoneType zone_type, int zone_capacity)
    {
		this.zone_id = zone_id;
		this.zone_name = "basic zone name";
		this.zone_type = zone_type;
		this.zone_level = 1;
		this.zone_population = 0;
		this.zone_employment = 0;
		this.zone_happiness = 100;
		this.zone_capacity = zone_capacity;
		this.zone_buildings = new List<Block>();
	}

    
}
