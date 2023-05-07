using Assets.Model.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

class BuildingPlacer
{
	private GameData gameData;
	private Timer timer;
	private int buildInterval;
	private int checkingFrequency;
	private int counter;
	private List<Vec2> buildablePositions;
	private Dictionary<ZoneType, List<Vec2>> availableBuildingSizes;

	public BuildingPlacer(GameData gameData, Dictionary<ZoneType, List<Vec2>> _availableBuildingSizes, int buildInterval = 1000, int checkingFrequency = 10)
	{
		this.gameData = gameData;
		this.buildInterval = buildInterval;
		this.checkingFrequency = checkingFrequency;
		this.counter = 0;
		this.buildablePositions = new List<Vec2>();
		this.availableBuildingSizes = _availableBuildingSizes;

		timer = new Timer(this.buildInterval);
		timer.Elapsed += OnTimedEvent;
		timer.AutoReset = true;
		timer.Enabled = true;
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e)
	{
		try { PlaceBuildingsOverTime(); } 
		catch (Exception ex){
			gameData.DebugInUnity(this,"An error occurred: " + ex.Message);
		}
		
	}

	public void ExitTimeEvent()
	{
		timer.Stop();
		timer.Dispose();
	}


	private void PlaceBuildingsOverTime()
	{
		if (checkingFrequency == counter)
		{
			buildablePositions = new List<Vec2>();
			counter = 0;
			for (int x = 0; x < gameData.gridWidth; x++)
			{
				for (int z = 0; z < gameData.gridHeight; z++)
				{
					if (gameData.grid[x][z].zoneType != ZoneType.Empty && gameData.grid[x][z].zoneType != ZoneType.Road && gameData.grid[x][z].zoneType != ZoneType.Water && gameData.grid[x][z].zoneType != ZoneType.IncomingRoad && gameData.grid[x][z].block == null )
					{
						// Check if there's a road in the vicinity
						if (gameData.IsConnectedRoad(x - 1, z) || gameData.IsConnectedRoad(x + 1, z) || gameData.IsConnectedRoad(x, z - 1) || gameData.IsConnectedRoad(x, z + 1))
						{
							buildablePositions.Add(new Vec2((uint)x, (uint)z));
						}
					}
				}
			}
		}
		counter++;

		if (buildablePositions.Count > 0)
		{
			// Choose a random buildable position
			Vec2 randomPosition = buildablePositions[RandomRange(0, buildablePositions.Count)];

			ZoneType selectedZonetype = gameData.grid[(int)randomPosition.x][(int)randomPosition.y].zoneType;
			BlockType blocktype;

			if (selectedZonetype == ZoneType.Residential)
			{
				blocktype = BlockType.House;
			}
			else if (selectedZonetype == ZoneType.Commercial)
			{
				blocktype = BlockType.Shop;
			}
			else
			{
				blocktype = BlockType.Factory;
			}

			// Place a building at the random position
			
			PlaceBuilding((int)randomPosition.x, (int)randomPosition.y, gameData.grid[(int)randomPosition.x][(int)randomPosition.y].zoneType, blocktype);
			gameData.DebugInUnity(this,"building placed at " + randomPosition.x + ", " + randomPosition.y + "+ current buildable positions: " + buildablePositions.Count);
		}
	}

	private int RandomRange(int min, int max)
	{
		Random random = new Random();
		return random.Next(min, max);
	}

	private void PlaceBuilding(int x, int z, ZoneType zoneType, BlockType blocktype)
	{
		// Check if a building is already placed
		if (gameData.grid[x][z].block != null) return;

		// Determine the largest possible building size for the given position
		int maxSize = GetMaxSize(x, z, zoneType);

		gameData.DebugInUnity(this,"maxsize calculated for " + blocktype + " in position " + x + ", " + z);

		// Get a random building prefab of the appropriate size for the zone type
		List<Vec2> suitableSizes = availableBuildingSizes[zoneType].Where(block =>
		{
			return block.x <= maxSize && block.y <= maxSize;
		}).ToList();
		Random rand = new Random();
		Vec2 buildingPrefab = suitableSizes[rand.Next(0, suitableSizes.Count)];

		gameData.DebugInUnity(this,"random building selected for " + blocktype + " in position " + x + ", " + z);

		// Get road direction
		int roadDirection = GetRoadDirection(x, z);

		// Instantiate the building
		int buildingSizeX;
		int buildingSizeZ;

		if (roadDirection % 2 == 1)
		{
			buildingSizeX = (int)buildingPrefab.y;
			buildingSizeZ = (int)buildingPrefab.x;
		}
		else
		{
			buildingSizeX = (int)buildingPrefab.x;
			buildingSizeZ = (int)buildingPrefab.y;
		}

		int chosenQuadrant = GetQuadrant(x, z, maxSize, zoneType);

		gameData.DebugInUnity(this,"getquadrant calculated for " + blocktype + " in position " + x + ", " + z);

		Block buildingInstance = new Block()
		{
			type = blocktype,
			blockSize = new Vec2((uint)buildingSizeX, (uint)buildingSizeZ)			
		};
		buildingInstance.setDefaultValues();
		gameData.grid[x][z].block = buildingInstance;

		gameData.DebugInUnity(this,"block set in grid for " + blocktype + " in position " + x + ", " + z);

		// Update the other grid cells that the building spawns on top of
		for (int offsetX = 0; offsetX < buildingSizeX; offsetX++)
		{
			for (int offsetZ = 0; offsetZ < buildingSizeZ; offsetZ++)
			{
				if (offsetX == 0 && offsetZ == 0) continue; // Skip the original grid cell

				gameData.DebugInUnity(this,"building placed and set the cells for it, " + blocktype + " in position " + x + ", " + z);

				int affectedX = x + (chosenQuadrant == 1 || chosenQuadrant == 3 ? -offsetX : offsetX);
				int affectedZ = z + (chosenQuadrant == 2 || chosenQuadrant == 3 ? -offsetZ : offsetZ);

				if (affectedX >= 0 && affectedZ >= 0 && affectedX < gameData.gridWidth && affectedZ < gameData.gridHeight)
				{
					gameData.grid[affectedX][affectedZ].block = buildingInstance;
				}
			}
		}

		//raise place building event
		gameData.BuildingPlaced(x, z, buildingInstance);

		gameData.DebugInUnity(this,"building placed of type " + blocktype + " in position " + x + ", " + z);
	}

	private int GetMaxSize(int x, int z, ZoneType zoneType)
	{
		int maxSize = 1;

		for (int size = 2; size <= 6; size++)
		{
			int quadrantSizes = 0;
			bool anyQuadrantSucceeded = false;

			for (int quadrant = 0; quadrant < 4; quadrant++)
			{
				bool canPlace = true;
				for (int offsetX = 0; offsetX < size; offsetX++)
				{
					for (int offsetZ = 0; offsetZ < size; offsetZ++)
					{
						int adjustedX = x + (quadrant == 1 || quadrant == 3 ? -offsetX : offsetX);
						int adjustedZ = z + (quadrant == 2 || quadrant == 3 ? -offsetZ : offsetZ);

						if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= gameData.gridWidth || adjustedZ >= gameData.gridHeight || gameData.grid[adjustedX][adjustedZ].zoneType != zoneType || gameData.grid[adjustedX][adjustedZ].block != null)
						{
							canPlace = false;
							break;
						}
					}
					if (!canPlace) break;
				}

				if (canPlace)
				{
					quadrantSizes = size;
					anyQuadrantSucceeded = true;
				}
			}

			if (anyQuadrantSucceeded)
			{
				maxSize = quadrantSizes;
			}
			else
			{
				break;
			}
		}

		return maxSize;
	}

	private int GetRoadDirection(int x, int z)
	{
		int[] dx = { 1, 0, -1, 0 };
		int[] dz = { 0, 1, 0, -1 };

		for (int dir = 0; dir < 4; dir++)
		{
			int newX = x + dx[dir];
			int newZ = z + dz[dir];

			if (newX >= 0 && newZ >= 0 && newX < gameData.gridWidth && newZ < gameData.gridHeight && gameData.grid[newX][newZ].zoneType == ZoneType.Road)
			{
				return dir;
			}
		}

		return -1;
	}

	private int GetQuadrant(int x, int z, int maxSize, ZoneType zoneType)
	{
		for (int quadrant = 0; quadrant < 6; quadrant++)
		{
			bool canPlace = true;
			for (int offsetX = 0; offsetX < maxSize; offsetX++)
			{
				for (int offsetZ = 0; offsetZ < maxSize; offsetZ++)
				{
					int adjustedX = x + (quadrant == 1 || quadrant == 3 ? -offsetX : offsetX);
					int adjustedZ = z + (quadrant == 2 || quadrant == 3 ? -offsetZ : offsetZ);

					if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= gameData.gridWidth || adjustedZ >= gameData.gridHeight || gameData.grid[adjustedX][adjustedZ].zoneType != zoneType || gameData.grid[adjustedX][adjustedZ].block != null)
					{
						canPlace = false;
						break;
					}
				}
				if (!canPlace) break;
			}

			if (canPlace)
			{
				return quadrant;
			}
		}

		return -1;
	}


}
