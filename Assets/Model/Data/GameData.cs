using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class GameData
    {
        public int balance;
        public int loans;
        public List<List<Field>> grid;
        public int gridWidth;
		public int gridHeight;
        public List<Citizen> citizens;
        public Time time;
		public BuildingPlacer buildingPlacer;
		public Dictionary<ZoneType, List<Vec2>> availableBuildingSizes;
		public CityLogic cityLogic;

		private int nextZone = 1;

		public delegate void ZoneTypeChangedEventHandler(int x, int z, ZoneType newZoneType);
		public delegate void BuildingPlacedEventHandler(int x, int z, Block buildingInstance);
		public delegate void DebugEventHandler(object ob, string message);


		public event ZoneTypeChangedEventHandler OnZoneTypeChanged;
		public event BuildingPlacedEventHandler OnBuildingPlaced;
		public event DebugEventHandler OnDebug;


		public GameData()
		{
			availableBuildingSizes = new Dictionary<ZoneType, List<Vec2>>();
			availableBuildingSizes.Add(ZoneType.Residential, new List<Vec2> { new Vec2(1, 1), new Vec2(2, 2), new Vec2(3,3) });
			availableBuildingSizes.Add(ZoneType.Commercial, new List<Vec2> { new Vec2(1, 1) });
			availableBuildingSizes.Add(ZoneType.Industrial, new List<Vec2> { new Vec2(1, 1) });
			buildingPlacer = new BuildingPlacer(this, availableBuildingSizes);
			citizens = new List<Citizen>();
			cityLogic = new CityLogic(this);
			loans = 0;
			balance = 10000;
			time = new Time();
		}

		public void SetUpGrid(int _gridWith, int _gridHeight)
		{
			gridWidth = _gridWith;
            gridHeight = _gridHeight;

			grid = new List<List<Field>>();


			if (gridHeight < 10 || gridWidth < 10)
            {
				throw new Exception("Grid size too small");
			}

			for (int i = 0; i < gridWidth; i++)
			{
				grid.Add(new List<Field>());

				for (int j = 0; j < gridHeight; j++)
				{
					Field cell = new Field
					{
						pos = new Vec2((uint)i, (uint)j),
						zoneType = ZoneType.Empty // Set the default zone type
					};

					grid[i].Add(cell);

				}
			}

			//PlaceIncomingRoad();
			PlaceIncomingRoadCenter();

			DebugInUnity(this,"grid set up is finished");
		}

        public void PlaceIncomingRoad()
		{
			// Create a random Incoming road at the edge of the grid
			Random rand = new Random();
			int randomSide = rand.Next(0, 4);
			int randomPosition = rand.Next(1, Math.Min(gridWidth+1, gridHeight) - 2);

			// Set the zone type to IncomingRoad for the random road position

			if (randomSide == 0)
			{
				ChangeZoneType(randomPosition, 0, ZoneType.IncomingRoad, 0);
				ChangeZoneType(randomPosition + 1, 0, ZoneType.Water, 0);
				ChangeZoneType(randomPosition - 1, 0, ZoneType.Water, 0);
			}
			else if (randomSide == 1)
			{
				ChangeZoneType(gridWidth - 1, randomPosition, ZoneType.IncomingRoad, 0);
				ChangeZoneType(gridWidth - 1, randomPosition - 1, ZoneType.Water, 0);
				ChangeZoneType(gridWidth - 1, randomPosition + 1, ZoneType.Water, 0);
			}
			else if (randomSide == 2)
			{
				ChangeZoneType(randomPosition, gridHeight - 1, ZoneType.IncomingRoad, 0);
				ChangeZoneType(randomPosition + 1, gridHeight - 1, ZoneType.Water, 0);
				ChangeZoneType(randomPosition - 1, gridHeight - 1, ZoneType.Water, 0);
			}
			else if (randomSide == 3)
			{
				ChangeZoneType(0, randomPosition, ZoneType.IncomingRoad, 0);
				ChangeZoneType(0, randomPosition - 1, ZoneType.Water, 0);
				ChangeZoneType(0, randomPosition + 1, ZoneType.Water, 0);
			}
		}

		public void PlaceIncomingRoadCenter()
        {
			ChangeZoneType(gridWidth / 2, 0, ZoneType.IncomingRoad, 0);
			ChangeZoneType(gridWidth / 2 + 1, 0, ZoneType.Water, 0);
			ChangeZoneType(gridWidth / 2 - 1, 0, ZoneType.Water, 0);
		}

		public bool isFieldValid(int x, int z)
		{
			if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight)
				return false;
			return true;
		}

		public void ChangeZoneType(int x, int z, ZoneType _zoneType, int _zoneId)
		{
			if (!isFieldValid(x, z)) return;
			
			if (_zoneType == ZoneType.Water || _zoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].zoneType = _zoneType;
				OnZoneTypeChanged?.Invoke(x, z, _zoneType);
			}

			if (grid[x][z].zoneType == _zoneType ||
				(grid[x][z].zoneType == ZoneType.Residential && _zoneType == ZoneType.Empty) ||
				(grid[x][z].zoneType == ZoneType.Commercial && _zoneType == ZoneType.Empty) ||
				(grid[x][z].zoneType == ZoneType.Industrial && _zoneType == ZoneType.Empty) ||
				(grid[x][z].zoneType == ZoneType.Road && _zoneType != ZoneType.Empty) ||
				grid[x][z].zoneType == ZoneType.IncomingRoad ||
				grid[x][z].zoneType == ZoneType.Water ||
				grid[x][z].block != null)
            {
				return;
			}


			grid[x][z].zoneType = _zoneType;

			// Set zoneid and clear overlap zones
			UpdateZoneId(x, z, _zoneType, _zoneId);

			//raise zonetype change event
			OnZoneTypeChanged?.Invoke(x, z, _zoneType);

			if(_zoneType == ZoneType.Road)
			{
				UpdateRoadConnectivity(x, z);
			}

			DebugInUnity(this,"zone type changed to " + _zoneType.ToString() + " at " + x + " " + z);
		}

		public void ChangeZoneTypeRectangle(Vec2 start, Vec2 end, ZoneType zoneType, int zoneId)
        {
			// Pre check
			for (int x = (int)start.x; x <= (int)end.x; x++)
			{
				for (int z = (int)start.y; z <= (int)end.y; z++)
				{
					if (x >= 0 && x < this.gridWidth && z >= 0 && z < this.gridHeight && this.grid[x][z].zoneId > 0)
					{
						// todo fire popup event
						return;
					}
				}
			}

			for (int x = (int)start.x; x <= (int)end.x; x++)
			{
				for (int z = (int)start.y; z <= (int)end.y; z++)
				{
					if (x >= 0 && x < this.gridWidth && z >= 0 && z < this.gridHeight)
					{
						this.ChangeZoneType(x, z, zoneType, zoneId);
					}
				}
			}
		}

		public void DeleteZone(int id)
		{
			for (int i = 0; i < this.gridWidth; i++)
			{
				for (int j = 0; j < this.gridHeight; j++)
				{
					if (this.grid[i][j].zoneId == id)
					{
						this.grid[i][j].zoneId = 0;

						if(grid[i][j].zoneType == ZoneType.Residential || grid[i][j].zoneType == ZoneType.Commercial || grid[i][j].zoneType == ZoneType.Industrial)
                        {
							this.grid[i][j].zoneType = ZoneType.Empty;
							OnZoneTypeChanged?.Invoke(i, j, ZoneType.Empty);
						}
					}
				}
			}
		}

		public ZoneType GetZoneType(int id)
        {
			// TODO: zones in a dictionary

			for (int i = 0; i < this.gridWidth; i++)
			{
				for (int j = 0; j < this.gridHeight; j++)
				{
					if (this.grid[i][j].zoneId == id)
					{
						if (grid[i][j].zoneType == ZoneType.Residential || grid[i][j].zoneType == ZoneType.Commercial || grid[i][j].zoneType == ZoneType.Industrial)
						{
							return this.grid[i][j].zoneType;
						}
					}
				}
			}

			return ZoneType.Empty;
		}

		private void UpdateZoneId(int x, int z, ZoneType _zoneType, int _zoneId)
        {
			if (_zoneType == ZoneType.Residential || _zoneType == ZoneType.Commercial || _zoneType == ZoneType.Industrial)
			{
				grid[x][z].zoneId = _zoneId;
				nextZone = _zoneId + 1;
			}
			else if(_zoneType != ZoneType.Road)
			{
				grid[x][z].zoneId = 0;
			}
		}

		public bool IsRoad(int x, int z)
		{
			if (isFieldValid(x, z))
			{
				return (grid[x][z].zoneType == ZoneType.Road || grid[x][z].zoneType == ZoneType.IncomingRoad);
			}

			return false;
		}

		private void UpdateRoadConnectivity(int x, int z)
		{
			//cant remove connection yet!!!

			if (!isFieldValid(x, z)) return;

			if (ZoneType.Road != grid[x][z].zoneType || grid[x][z].IsConnectedToIncomingRoad) return;

			//check if any of the 4 adjacent cells are road with connection
			if (IsConnectedRoad(x - 1, z) || IsConnectedRoad(x + 1, z) || IsConnectedRoad(x, z - 1) || IsConnectedRoad(x, z + 1))
			{
				grid[x][z].IsConnectedToIncomingRoad = true;

				UpdateRoadConnectivity(x - 1, z);
				UpdateRoadConnectivity(x + 1, z);
				UpdateRoadConnectivity(x, z - 1);
				UpdateRoadConnectivity(x, z + 1);
			}
			else
			{
				grid[x][z].IsConnectedToIncomingRoad = false;
			}


		}

		public bool IsConnectedRoad(int x, int z)
		{
			if (!isFieldValid(x, z))
				return false;
			if (!IsRoad(x, z))
				return false;
			if (grid[x][z].IsConnectedToIncomingRoad)
				return true;
			if (grid[x][z].zoneType == ZoneType.IncomingRoad)
				return true;
			return false;
		}

		public void BuildingPlaced(int x, int z, Block buildingInstance)
		{
			//fire event
			DebugInUnity(this,"invoke event for building placed at " + x + " " + z);
			OnBuildingPlaced?.Invoke(x, z, buildingInstance);
		}

		public void DebugInUnity(object ob, string message)
		{
			// invoke event
			OnDebug?.Invoke(ob, message);
		}

		public void OnApplicationExit()
		{
			buildingPlacer.ExitTimeEvent();
		}

		public List<Block> GetBuildings()
		{
			List<Block> buildings = new List<Block>();
			foreach (List<Field> row in grid)
			{
				foreach (Field field in row)
				{
					if (!buildings.Contains(field.block))
					{
						buildings.Add(field.block);
					}
				}
			}
			return buildings;
		}

		public int getNextZoneId()
        {
			return nextZone;
        }

		public int getZoneId(int x, int z)
        {
			return grid[x][z].zoneId;
        }

		/*public int getOperatingCost()
        {
            return getBuildings().AsEnumerable().Sum(b => b.operating_cost);
        }

        public int getTotalLevel()
        {
            return getBuildings().AsEnumerable().Sum(b => b.lvl);
        }

        public List<Block> findBlocks(BlockType bt)
        {
            return grid.FindAll(b => b.type == bt);
        }

        public List<Block> getRoads()
        {
            return findBlocks(BlockType.Road);
        }

        public List<Block> getPoliceStation()
        {
            return findBlocks(BlockType.PoliceStation);
        }

        public List<Block> getPowerPlant()
        {
            return findBlocks(BlockType.PowerPlant);
        }

        public List<Block> getPowerLine()
        {
            return findBlocks(BlockType.PowerLine);
        }

        public List<Block> getStadium()
        {
            return findBlocks(BlockType.Stadium);
        }

        public List<Block> getSchool()
        {
            return findBlocks(BlockType.School);
        }

        public List<Block> getUniversity()
        {
            return findBlocks(BlockType.University);
        }

        public List<Block> getForest()
        {
            return findBlocks(BlockType.Forest);
        }*/
	}
}
