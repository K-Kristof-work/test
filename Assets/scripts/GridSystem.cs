using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class GridSystem : MonoBehaviour
{
	#region public variables

	public List<List<GridCell>> grid = new List<List<GridCell>>();
	public float cellWidth; // Set the desired cell width.
	public float cellHeight; // Set the desired cell height.
	public int gridWidth; // Set the desired grid width.
	public int gridHeight; // Set the desired grid height.
	public GameObject cellPrefab;
	public Transform gridParent; // Drag and drop the "GridSystem" GameObject to this field in the Inspector
	public GameObject roadMeshPrefab;
	public GameObject IncomingRoadMeshPrefab;
	public GameObject WaterMeshPrefab;

	//make list for all of the road meshes
	public List<Mesh> StraightRoad = new List<Mesh>();
	public List<Mesh> CornerRoad = new List<Mesh>();
	public List<Mesh> TIntersectionRoad = new List<Mesh>();
	public List<Mesh> CrossRoad = new List<Mesh>();

	public Dictionary<int,List<Mesh>> roadMeshes;


	public float buildInterval = 0.5f;
	public List<GameObject> residentialBuildingPrefabs;
	public List<GameObject> commercialBuildingPrefabs;
	public List<GameObject> industrialBuildingPrefabs;

	#endregion

	#region private variables

	private Dictionary<ZoneType, List<GameObject>> zoneTypeToBuildingPrefabs;

	//public List<List<Mesh>>  roadMeshes;
	private ZoneMaterials zoneMaterials;
	private bool isroadupdating = false;

	#endregion

	#region public properties

	public int GridWidth { get { return gridWidth; } }
	public int GridHeight { get { return gridHeight; } }
	public float CellWidth { get { return cellWidth; } }
	public float CellHeight { get { return cellHeight;   } }

	#endregion

	#region unity methods
	public void Awake()
	{
		roadMeshes = new Dictionary<int, List<Mesh>>
		{
			{0, StraightRoad},
			{1, CornerRoad},
			{2, TIntersectionRoad},
			{3, CrossRoad},
		};

		zoneTypeToBuildingPrefabs = new Dictionary<ZoneType, List<GameObject>>
		{
			{ZoneType.Residential,  residentialBuildingPrefabs},
			{ZoneType.Commercial, commercialBuildingPrefabs},
			{ZoneType.Industrial, industrialBuildingPrefabs}

		};

	}
	void Start()
	{
		zoneMaterials = GetComponent<ZoneMaterials>();
		SetUpGrid();
		StartCoroutine(PlaceBuildingsOverTime());
	}

	#endregion

	#region methods for grid creation and zone managment

	#region grid creation
	public void SetUpGrid()
	{
		// Create a random Incoming road at the edge of the grid
		int randomSide = Random.Range(0, 4);
		int randomPosition = Random.Range(1, Mathf.Min(gridWidth, gridHeight)-1);

		for (int i = 0; i < gridWidth; i++)
		{
			grid.Add(new List<GridCell>());

			for (int j = 0; j < gridHeight; j++)
			{
				GridCell cell = new GridCell
				{
					Position = new Vector3(i * cellWidth, 0, j * cellHeight),
					Size = new Vector3(cellWidth / 10, cellWidth / 10, cellHeight / 10),
					ZoneType = ZoneType.Empty // Set the default zone type
				};

				grid[i].Add(cell);

				// Instantiate the cell prefab
				GameObject cellInstance = Instantiate(cellPrefab, cell.Position, Quaternion.identity, gridParent);
				cellInstance.transform.localScale = cell.Size;
				cellInstance.name = $"GridCell_{i}_{j}"; // Set the cell name with indices

				// Store the instantiated GameObject in the GridCell
				cell.CellObject = cellInstance;

				// Assign the correct material based on the zone type
				ZoneMaterials zoneMaterials = GetComponent<ZoneMaterials>();
				Material zoneMaterial = zoneMaterials.GetRandomMaterial(cell.ZoneType);
				MeshRenderer renderer = cellInstance.GetComponent<MeshRenderer>();
				if (renderer != null)
				{
					renderer.material = zoneMaterial;
				}

			}
		}

		// Set the zone type to IncomingRoad for the random road position

		if(randomSide == 0)
		{
			ChangeZoneType(randomPosition, 0, ZoneType.IncomingRoad);
			ChangeZoneType(randomPosition + 1, 0, ZoneType.Water);
			ChangeZoneType(randomPosition - 1, 0, ZoneType.Water);
		}
		else if(randomSide == 1)
		{
			ChangeZoneType(gridWidth - 1, randomPosition, ZoneType.IncomingRoad);
			ChangeZoneType(gridWidth - 1, randomPosition -1, ZoneType.Water);
			ChangeZoneType(gridWidth - 1, randomPosition + 1, ZoneType.Water);
		}
		else if(randomSide == 2)
		{
			ChangeZoneType(randomPosition, gridHeight - 1, ZoneType.IncomingRoad);
			ChangeZoneType(randomPosition + 1, gridHeight - 1, ZoneType.Water);
			ChangeZoneType(randomPosition - 1, gridHeight - 1, ZoneType.Water);
		}
		else if(randomSide == 3)
		{
			ChangeZoneType(0, randomPosition, ZoneType.IncomingRoad);
			ChangeZoneType(0, randomPosition - 1, ZoneType.Water);
			ChangeZoneType(0, randomPosition + 1, ZoneType.Water);
		}
	}
	#endregion

	#region zone managment

	public void ChangeZoneType(int x, int z, ZoneType zoneType)
	{
		if (!CheckGrid(x, z)) return;

		if (zoneType == ZoneType.IncomingRoad)
		{

			// Instantiate the roadMeshPrefab
			GameObject roadMeshObject = Instantiate(IncomingRoadMeshPrefab, grid[x][z].Position, IncomingRoadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			roadMeshObject.name = "IncomingRoadMesh";
			grid[x][z].ZoneType = zoneType;

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = roadMeshObject;
			if (z == 0 || z == gridWidth - 1)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																		  grid[x][z].Building.transform.rotation.y + 90,
																		  grid[x][z].Building.transform.rotation.z);
			}

			UpdateCell(x, z);
		}

		if (zoneType == ZoneType.Water)
		{
			// Instantiate the roadMeshPrefab
			GameObject WaterObject = Instantiate(WaterMeshPrefab, grid[x][z].Position, IncomingRoadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			WaterObject.name = "WaterMesh";
			grid[x][z].ZoneType = zoneType;

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = WaterObject;

			//set the rotation
			SetWaterRotation(x, z);
			UpdateCell(x, z);
		}

		if (grid[x][z].ZoneType == zoneType || 
			grid[x][z].ZoneType == ZoneType.Road || 
			grid[x][z].ZoneType == ZoneType.IncomingRoad || 
			grid[x][z].ZoneType == ZoneType.Water) 
				return;

		grid[x][z].ZoneType = zoneType;

		if (zoneType == ZoneType.Road && grid[x][z].Building == null)
		{

			// Instantiate the roadMeshPrefab
			GameObject roadMeshObject = Instantiate(roadMeshPrefab,grid[x][z].Position, roadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			roadMeshObject.name = "RoadMesh";

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = roadMeshObject;

			UpdateRoadMesh(x, z);
			updateRoadsAround(x, z);
			UpdateRoadConnectivity(x ,z);
		}
		else
		{
			// Check if the GridCell.Building is a road mesh and destroy it if so
			if (grid[x][z].Building != null)
			{
				if(grid[x][z].Building.name == "RoadMesh")
				{
					Destroy(grid[x][z].Building);
					grid[x][z].Building = null;
				}
				else
				{
					return;
				}
			}

			
		}

		UpdateCell(x, z);
	}

	public bool CheckGrid(int x, int z)
	{
		if (x < 0 || x >= GridWidth || z < 0 || z >= GridHeight)
			return false;
		return true;
	}

	public void SetWaterRotation(int x, int z)
	{
		if (x == 0)
		{
			if (grid[x][z + 1].ZoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y - 90,
																	  grid[x][z].Building.transform.rotation.z);
			}
			else
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y + 180,
																	  grid[x][z].Building.transform.rotation.z);
			}
		}
		else if (z == 0)
		{
			if (grid[x + 1][z].ZoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y + 90,
																	  grid[x][z].Building.transform.rotation.z);
			}
			else
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y + 180,
																	  grid[x][z].Building.transform.rotation.z);
			}
		}
		else if (z == gridWidth - 1)
		{
			if (grid[x + 1][z].ZoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y,
																	  grid[x][z].Building.transform.rotation.z);
			}
			else
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y - 90,
																	  grid[x][z].Building.transform.rotation.z);
			}
		}
		else
		{
			if (grid[x][z + 1].ZoneType == ZoneType.IncomingRoad)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y,
																	  grid[x][z].Building.transform.rotation.z);
			}
			else
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																	  grid[x][z].Building.transform.rotation.y + 90,
																	  grid[x][z].Building.transform.rotation.z);
			}
		}
	}

	public void UpdateCell(int xIndex, int zIndex)
	{
		GridCell cell = grid[xIndex][zIndex];

		GameObject cellObject = cell.CellObject;
		if (cellObject != null)
		{
			//make a debug log that give information on cell

			Material zoneMaterial = zoneMaterials.GetRandomMaterial(cell.ZoneType);



			MeshRenderer renderer = cellObject.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				renderer.material = zoneMaterial;
			}
		}
	}

	public void SetZone(Vector3 worldPosition, ZoneType SelectedZoneType)
	{
		int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / CellWidth + CellWidth/2);
		int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / CellHeight + CellHeight/2);
		ChangeZoneType(x, z, SelectedZoneType);

	}

	public void SetZoneRectangle(Vector3 start, Vector3 end, ZoneType SelectedZoneType)
	{
		float halfCellWidth = CellWidth * 0.5f;
		float halfCellHeight = CellHeight * 0.5f;
		int startX = Mathf.FloorToInt(Mathf.Min(start.x + halfCellWidth, end.x + halfCellWidth));
		int startZ = Mathf.FloorToInt(Mathf.Min(start.z + halfCellHeight, end.z + halfCellHeight));
		int endX = Mathf.FloorToInt(Mathf.Max(start.x + halfCellWidth, end.x + halfCellWidth));
		int endZ = Mathf.FloorToInt(Mathf.Max(start.z + halfCellHeight, end.z + halfCellHeight));

		for (int x = startX; x <= endX; x++)
		{
			for (int z = startZ; z <= endZ; z++)
			{
				if (x >= 0 && x < GridWidth && z >= 0 && z < GridHeight)
				{
					ChangeZoneType(x, z, SelectedZoneType);
				}
			}
		}
	}

	public void OnCellClicked(int xIndex, int zIndex, ZoneType newZoneType)
	{
		ChangeZoneType(xIndex, zIndex, newZoneType);
	}

	#endregion

	#region road managment
	public void updateRoadsAround(int x, int z)
	{
		UpdateRoadMesh(x - 1, z);
		UpdateRoadMesh(x + 1, z);
		UpdateRoadMesh(x, z - 1);
		UpdateRoadMesh(x, z + 1);
	}

	Mesh GetRoadMesh(int meshNum)
	{
		//make sure the meshNum is within the range of the roadMeshes array
		if (meshNum < 0 || meshNum >= roadMeshes.Count)
		{
			return null;
		}

        int randomNum = Random.Range(0, roadMeshes[meshNum].Count);

		//give back a random mesh
		return roadMeshes[meshNum][randomNum];
		

	}

	private void UpdateRoadMesh(int x, int z)
	{
		if (!CheckGrid(x, z)) return;
		
		if (grid[x][z].ZoneType== ZoneType.Road && !isroadupdating)
		{
			isroadupdating = true;
			updateRoadsAround(x, z);

			if( x == 10 && z ==10)
				{
				}

			GridCell cell = grid[x][z];
			int roadMask = CalculateRoadMask(x, z);

			// Get the road mesh based on the roadMask value
			Mesh roadMesh = null;
			float rotationY = 0;
			switch (roadMask)
			{
				case 0:
				case 1:
				case 2:
				case 4:
				case 5:
				case 8:
				case 10:
				 // Straight
					roadMesh = GetRoadMesh(0);
					rotationY = (roadMask == 1 || roadMask == 4 || roadMask == 5) ? 0 : 90;
					break;
				case 3:
				case 6:
				case 9:
				case 12: // Corner
					roadMesh = GetRoadMesh(1);
					rotationY = (roadMask == 3) ? 90 : (roadMask == 6) ? 180 : (roadMask == 9) ? 0 : 270;
					break;
				case 7:
				case 11:
				case 13:
				case 14: // T-junction
					roadMesh = GetRoadMesh(2);
					rotationY = (roadMask == 7) ? 0 : (roadMask == 11) ? 270 : (roadMask == 13) ? 180 : 90;
					break;
				case 15: // 4-way
					roadMesh = GetRoadMesh(3);
					break;
			}

			// Set the road mesh
			MeshRenderer meshrenderer = cell.Building.GetComponent<MeshRenderer>();

			MeshFilter roadMeshFilter = meshrenderer.GetComponent<MeshFilter>();
			roadMeshFilter.mesh = roadMesh;

			// Set the road mesh rotation
			meshrenderer.transform.rotation = Quaternion.Euler(-90, rotationY, 0);

			// Enable the road mesh renderer if the zone type is Road
			meshrenderer.enabled = true;
			isroadupdating = false;
		}
		
	}

	private int CalculateRoadMask(int x, int z)
	{
		int roadMask = 0;

		if (IsRoad(x, z + 1)) // north
		{
			roadMask += 1;
		}
		if (IsRoad(x + 1, z)) // East
		{
			roadMask += 2;
		}
		if (IsRoad(x, z - 1)) // South
		{
			roadMask += 4;
		}
		if (IsRoad(x - 1, z)) // West
		{
			roadMask += 8;
		}
		return roadMask;
	}

	private bool IsRoad(int x, int z)
	{
		if (CheckGrid(x, z))
		{
			return (grid[x][z].ZoneType == ZoneType.Road || grid[x][z].ZoneType == ZoneType.IncomingRoad);
		}

		return false;
	}

	private void UpdateRoadConnectivity(int x, int z)
	{
		//cant remove connection yet!!!

		if (!CheckGrid(x,z)) return;

		if (ZoneType.Road != grid[x][z].ZoneType || grid[x][z].IsConnectedToIncomingRoad) return;

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
		if (!CheckGrid(x, z) && IsRoad(x, z)) 
			return false;
		if (grid[x][z].IsConnectedToIncomingRoad)
			return true;
		if (grid[x][z].ZoneType == ZoneType.IncomingRoad)
			return true;
		return false;
	}

	#endregion

	#endregion

	#region methods for buildings

	private IEnumerator PlaceBuildingsOverTime()
	{
		WaitForSeconds wait = new WaitForSeconds(buildInterval);
		List<Vector2Int> buildablePositions = new List<Vector2Int>();
		int checkingFrequency = 10; // bigger is fewer checks
		int counter = 0;
		while (true)
		{


			if (checkingFrequency == counter)
			{
				buildablePositions = new List<Vector2Int>();
				counter = 0;
				for (int x = 0; x < GridWidth; x++)
				{
					for (int z = 0; z < GridHeight; z++)
					{
						if (grid[x][z].ZoneType != ZoneType.Empty && grid[x][z].ZoneType != ZoneType.Road && grid[x][z].Building == null)
						{
							// Check if there's a road in the vicinity
							if (IsConnectedRoad(x - 1, z) || IsConnectedRoad(x + 1, z) || IsConnectedRoad(x, z - 1) || IsConnectedRoad(x, z + 1))
							{
									buildablePositions.Add(new Vector2Int(x, z));
							}
						}

						
					}
				}
			}
			counter++;
			if (buildablePositions.Count > 0)
			{
				// Choose a random buildable position
				Vector2Int randomPosition = buildablePositions[Random.Range(0, buildablePositions.Count)];

				// Place a building at the random position
				PlaceBuilding(randomPosition.x, randomPosition.y, grid[randomPosition.x][randomPosition.y].ZoneType);
			}

			yield return wait;
		}
	}


	private void PlaceBuilding(int x, int z, ZoneType zoneType)
	{
		// Check if a building is already placed
		if (grid[x][z].Building != null) return;

		// Determine the largest possible building size for the given position
		int maxSize = GetMaxSize(x, z, zoneType);

		// Get a random building prefab of the appropriate size for the zone type
		List<GameObject> suitablePrefabs = zoneTypeToBuildingPrefabs[zoneType].Where(prefab =>
		{
			BuildingPrefab prefabScript = prefab.GetComponent<BuildingPrefab>();
			return prefabScript.BuildingSize.x <= maxSize && prefabScript.BuildingSize.y <= maxSize;
		}).ToList(); 
		GameObject buildingPrefab = suitablePrefabs[Random.Range(0, suitablePrefabs.Count)];

		// Get road direction
		int roadDirection = GetRoadDirection(x, z);


		// Set building rotation to face the road
		BuildingPrefab prefabScript = buildingPrefab.GetComponent<BuildingPrefab>();
		prefabScript.rotationdirection = roadDirection;
		Quaternion buildingRotation;
		if (roadDirection != -1)
		{
			if (roadDirection % 2 == 0)
				roadDirection += 2;
			buildingRotation = Quaternion.Euler(prefabScript.rotationInMeshOnX, -90 + (roadDirection + 1) * 90, 0);
		}
		else
		{
			buildingRotation = buildingPrefab.transform.rotation;
		}

		// Instantiate the building
		int buildingSizeX;
		int buildingSizeZ;

		if (roadDirection % 2 == 1)
		{
			buildingSizeX = prefabScript.BuildingSize.y;
			buildingSizeZ = prefabScript.BuildingSize.x;
		}
		else
		{
			buildingSizeX = prefabScript.BuildingSize.x;
			buildingSizeZ = prefabScript.BuildingSize.y;
		}

		float buildingPosY = buildingPrefab.transform.position.y;
		int chosenQuadrant = -1;
		Vector3 centerPosition = GetCenterPosition(x, z, maxSize, buildingSizeX, buildingSizeZ, buildingPosY, zoneType, ref chosenQuadrant);



		GameObject buildingInstance = Instantiate(buildingPrefab, centerPosition, buildingRotation, grid[x][z].CellObject.transform);
		grid[x][z].Building = buildingInstance;
		grid[x][z].Building.GetComponent<BuildingPrefab>().BuildingSize.x = buildingSizeX;
		grid[x][z].Building.GetComponent<BuildingPrefab>().BuildingSize.y = buildingSizeZ;

		// Update the other grid cells that the building spawns on top of
		for (int offsetX = 0; offsetX < buildingSizeX; offsetX++)
		{
			for (int offsetZ = 0; offsetZ < buildingSizeZ; offsetZ++)
			{
				if (offsetX == 0 && offsetZ == 0) continue; // Skip the original grid cell

				int affectedX = x + (chosenQuadrant == 1 || chosenQuadrant == 3 ? -offsetX : offsetX);
				int affectedZ = z + (chosenQuadrant == 2 || chosenQuadrant == 3 ? -offsetZ : offsetZ);

				if (affectedX >= 0 && affectedZ >= 0 && affectedX < GridWidth && affectedZ < GridHeight)
				{
					grid[affectedX][affectedZ].Building = buildingInstance;
				}
			}
		}
		// Attaching the BuildingAnimationController script to the building will play the animation
		// buildingInstance.AddComponent<BuildAnimationController>();

	}

	private int GetRoadDirection(int x, int z)
	{
		int[] dx = { 1, 0, -1, 0 };
		int[] dz = { 0, 1, 0, -1 };

		for (int dir = 0; dir < 4; dir++)
		{
			int newX = x + dx[dir];
			int newZ = z + dz[dir];

			if (newX >= 0 && newZ >= 0 && newX < GridWidth && newZ < GridHeight && grid[newX][newZ].ZoneType == ZoneType.Road)
			{
				return dir;
			}
		}

		return -1;
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

						if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= GridWidth || adjustedZ >= GridHeight || grid[adjustedX][adjustedZ].ZoneType != zoneType || grid[adjustedX][adjustedZ].Building != null)
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


	private Vector3 GetCenterPosition(int x, int z, int maxSize, int buildingSizeX, int buildingSizeZ, float buildingPosY, ZoneType zoneType, ref int chosenQuadrant)
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

					if (adjustedX < 0 || adjustedZ < 0 || adjustedX >= GridWidth || adjustedZ >= GridHeight || grid[adjustedX][adjustedZ].ZoneType != zoneType || grid[adjustedX][adjustedZ].Building != null)
					{
						canPlace = false;
						break;
					}
				}
				if (!canPlace) break;
			}

			if (canPlace)
			{
				float xOffset = ((buildingSizeX - 1) / 2f) * (quadrant == 1 || quadrant == 3 ? -1 : 1);
				float zOffset = ((buildingSizeZ - 1) / 2f) * (quadrant == 2 || quadrant == 3 ? -1 : 1);
				chosenQuadrant = quadrant;
				return grid[x][z].Position + new Vector3(xOffset, buildingPosY/10, zOffset); // divide per 10 to make it work dont kn ow why just dont touch it
			}
		}

		return new Vector3(grid[x][z].Position.x,buildingPosY, grid[x][z].Position.z);
	}

	#endregion
}

public class GridCell
{
	#region variables

	public Vector3 Position { get; set; }
	public Vector3 Size { get; set; }
	public GameObject Building { get; set; }
	public ZoneType ZoneType { get; set; }
	public GameObject CellObject { get; set; }
	public bool IsConnectedToIncomingRoad = false;

	#endregion
}

