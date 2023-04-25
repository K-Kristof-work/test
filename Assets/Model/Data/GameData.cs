using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Data
{
    class GameData
    {
        public int balance;
        public int[] taxes;
        public int loans;
        public List<List<Field>> grid;
        public int gridWidth;
		public int gridHeight;
        public List<Citizen> citizens;
        public Time time;
		private Dictionary<ZoneType, List<Vec2>> availableBuildingSizes;

		public void SetUpGrid(int _gridWith, int _gridHeight)
		{
			gridWidth = _gridWith;
            gridHeight = _gridHeight;

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

            PlaceIncomingRoad();
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
				ChangeZoneType(randomPosition, 0, ZoneType.IncomingRoad);
				ChangeZoneType(randomPosition + 1, 0, ZoneType.Water);
				ChangeZoneType(randomPosition - 1, 0, ZoneType.Water);
			}
			else if (randomSide == 1)
			{
				ChangeZoneType(gridWidth - 1, randomPosition, ZoneType.IncomingRoad);
				ChangeZoneType(gridWidth - 1, randomPosition - 1, ZoneType.Water);
				ChangeZoneType(gridWidth - 1, randomPosition + 1, ZoneType.Water);
			}
			else if (randomSide == 2)
			{
				ChangeZoneType(randomPosition, gridHeight - 1, ZoneType.IncomingRoad);
				ChangeZoneType(randomPosition + 1, gridHeight - 1, ZoneType.Water);
				ChangeZoneType(randomPosition - 1, gridHeight - 1, ZoneType.Water);
			}
			else if (randomSide == 3)
			{
				ChangeZoneType(0, randomPosition, ZoneType.IncomingRoad);
				ChangeZoneType(0, randomPosition - 1, ZoneType.Water);
				ChangeZoneType(0, randomPosition + 1, ZoneType.Water);
			}
		}

		public bool isFieldValid(int x, int z)
		{
			if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight)
				return false;
			return true;
		}

		public void ChangeZoneType(int x, int z, ZoneType _zoneType)
		{
			if (!isFieldValid(x, z)) return;
			
			if (_zoneType == ZoneType.Water || _zoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].zoneType = _zoneType;
			}

			if (grid[x][z].zoneType == _zoneType ||
				grid[x][z].zoneType == ZoneType.Road ||
				grid[x][z].zoneType == ZoneType.IncomingRoad ||
				grid[x][z].zoneType == ZoneType.Water ||
				grid[x][z].block != null)
				return;

			grid[x][z].zoneType = _zoneType;
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

		private bool IsConnectedRoad(int x, int z)
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

		private void PlaceBuilding(int x, int z, ZoneType zoneType)
		{
			// Check if a building is already placed
			if (grid[x][z].block != null) return;

			// Determine the largest possible building size for the given position
			int maxSize = GetMaxSize(x, z, zoneType);

			// Get a random building prefab of the appropriate size for the zone type
			List<Vec2> suitableSizes = availableBuildingSizes[zoneType].Where(block =>
			{
				return block.x <= maxSize && block.y <= maxSize;
			}).ToList();
			Random rand = new Random();
			Vec2 buildingPrefab = suitableSizes[rand.Next(0, suitableSizes.Count)];

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



			Block buildingInstance = new Block();
			grid[x][z].block = buildingInstance;
			grid[x][z].block.blockSize.x = (uint)buildingSizeX;
			grid[x][z].block.blockSize.y = (uint)buildingSizeZ;

			// Update the other grid cells that the building spawns on top of
			for (int offsetX = 0; offsetX < buildingSizeX; offsetX++)
			{
				for (int offsetZ = 0; offsetZ < buildingSizeZ; offsetZ++)
				{
					if (offsetX == 0 && offsetZ == 0) continue; // Skip the original grid cell

					int affectedX = x + (chosenQuadrant == 1 || chosenQuadrant == 3 ? -offsetX : offsetX);
					int affectedZ = z + (chosenQuadrant == 2 || chosenQuadrant == 3 ? -offsetZ : offsetZ);

					if (affectedX >= 0 && affectedZ >= 0 && affectedX < gridWidth && affectedZ < gridHeight)
					{
						grid[affectedX][affectedZ].block = buildingInstance;
					}
				}
			}

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

							if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= gridWidth || adjustedZ >= gridHeight || grid[adjustedX][adjustedZ].zoneType != zoneType || grid[adjustedX][adjustedZ].block != null)
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

				if (newX >= 0 && newZ >= 0 && newX < gridWidth && newZ < gridHeight && grid[newX][newZ].zoneType == ZoneType.Road)
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

						if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= gridWidth || adjustedZ >= gridHeight || grid[adjustedX][adjustedZ].zoneType != zoneType || grid[adjustedX][adjustedZ].block != null)
						{
							canPlace = false;
							break;
						}
					}
					if (!canPlace) break;
				}

				if (canPlace)
				{
					return quadrant;								}
			}

			return -1;
		}

		/*public List<Block> GetBuildings()
		{
			var flattenedGrid = grid.SelectMany(g => g).ToList();
			return flattenedGrid
				.FindAll(b => GameConfig.Buildings.Contains(b.zoneType))
				.Select(f => new Block { zonetype = f.zoneType })
				.ToList();
		}

		public int getOperatingCost()
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
