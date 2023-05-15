using Assets.Model.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System;
using System.Timers;

public class ZoneData
{
	private Timer timer;
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
    public List<Field> zone_fields;

    private BuildingPlacer bp;
	private int counter = 0;
	private List<Vec2> buildablePositions;
	private GameData gameData;

	//make a constructor that takes in a all the values and sets all the values
	public ZoneData(int zone_id, ZoneType zone_type, int zone_capacity, BuildingPlacer _bp, GameData gm)
    {
		this.zone_id = zone_id;
		this.zone_name = "basic zone name";
		this.zone_type = zone_type;
		this.zone_level = 1;
		this.zone_population = 0;
		this.zone_employment = 0;
		this.zone_happiness = 100;
		this.zone_tax = 20;
		this.zone_capacity = zone_capacity;
		this.zone_buildings = new List<Block>();
        this.bp = _bp;
		this.gameData = gm;
		zone_fields= new List<Field>();

		if(zone_type == ZoneType.Residential || zone_type == ZoneType.Commercial || zone_type == ZoneType.Industrial)
		{
			timer = new Timer(1);
			timer.Elapsed += OnTimedEvent;
			timer.AutoReset = true;
			timer.Enabled = true;
		}
		
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e)
	{
		try { PlaceBuildingsOverTimeInZone(); }
		catch (Exception ex)
		{
			gameData.DebugInUnity(this, "An error occurred: " + ex.Message);
		}

	}

	public void AddField(Field field)
    {
		zone_fields.Add(field);
	}

	private void PlaceBuildingsOverTimeInZone()
	{
		int checkingFrequency = 10;
		if (checkingFrequency == counter)
		{
			buildablePositions = new List<Vec2>();
			counter = 0;
			foreach (var field in zone_fields)
			{
				if (field.zone.zone_type != ZoneType.Empty && field.zone.zone_type != ZoneType.Road && field.zone.zone_type != ZoneType.Water && field.zone.zone_type != ZoneType.IncomingRoad && field.block == null)
				{
					int x = (int)field.pos.x;
					int z = (int)field.pos.y;

					// Check if there's a road in the vicinity
					if (gameData.IsConnectedRoad(x - 1, z) || gameData.IsConnectedRoad(x + 1, z) || gameData.IsConnectedRoad(x, z - 1) || gameData.IsConnectedRoad(x, z + 1))
					{
						buildablePositions.Add(new Vec2((uint)x, (uint)z));
					}
				}
			}
		}

		counter++;

		if (buildablePositions.Count > 0)
		{
			// Choose a random buildable position
			Vec2 randomPosition = buildablePositions[RandomRange(0, buildablePositions.Count)];
			BlockType blocktype;
			if (zone_type == ZoneType.Residential)
			{
				blocktype = BlockType.House;
			}
			else if (zone_type == ZoneType.Commercial)
			{
				blocktype = BlockType.Shop;
			}
			else
			{
				blocktype = BlockType.Factory;
			}

			// Place a building at the random position

			bp.PlaceBuilding((int)randomPosition.x, (int)randomPosition.y, zone_type, blocktype, zone_level);
			gameData.DebugInUnity(this, "building placed at " + randomPosition.x + ", " + randomPosition.y + "+ current buildable positions: " + buildablePositions.Count);
		}

		timer.Interval = gameData.cityLogic.SetZoneTimerInterval(this);

	}

	private int RandomRange(int min, int max)
	{
		Random random = new Random();
		return random.Next(min, max);
	}

	public void ExitTimeEvent()
	{
		if(timer!= null) {
			timer.Stop();
			timer.Dispose();
		}

	}




}
