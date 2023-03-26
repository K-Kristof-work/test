using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
	public List<List<GridCell>> grid = new List<List<GridCell>>();
	public float cellWidth; // Set the desired cell width.
	public float cellHeight; // Set the desired cell height.
	public int gridWidth; // Set the desired grid width.
	public int gridHeight; // Set the desired grid height.
	public GameObject cellPrefab;
	public Transform gridParent; // Drag and drop the "GridSystem" GameObject to this field in the Inspector
	public GameObject roadMeshPrefab;
	
	//make list for all of the road meshes
	public List<Mesh> StraightRoad = new List<Mesh>();
	public List<Mesh> CornerRoad = new List<Mesh>();
	public List<Mesh> TIntersectionRoad = new List<Mesh>();
	public List<Mesh> CrossRoad = new List<Mesh>();

	public Dictionary<int,List<Mesh>> roadMeshes;

	//public List<List<Mesh>>  roadMeshes;
	private ZoneMaterials zoneMaterials;
	private bool isroadupdating = false;

	public int GridWidth { get { return gridWidth; } }
	public int GridHeight { get { return gridHeight; } }
	public float CellWidth { get { return cellWidth; } }
	public float CellHeight { get { return cellHeight;   } }

	public void Awake()
	{
		roadMeshes = new Dictionary<int, List<Mesh>>
		{
			{0, StraightRoad},
			{1, CornerRoad},
			{2, TIntersectionRoad},
			{3, CrossRoad},
		};
	}
	void Start()
	{
		SetUpGrid();
		zoneMaterials = GetComponent<ZoneMaterials>();
	}
	public void SetUpGrid()
	{
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

				// Instantiate the road mesh prefab as a child of the cell
				GameObject roadMeshInstance = Instantiate(roadMeshPrefab, cell.Position, Quaternion.identity, cellInstance.transform);
				roadMeshInstance.transform.localScale = roadMeshPrefab.transform.localScale;
				roadMeshInstance.name = "RoadMesh";
				MeshRenderer roadMeshRenderer = roadMeshInstance.GetComponent<MeshRenderer>();
				roadMeshRenderer.enabled = false;

				// Store the instantiated road mesh renderer in the GridCell
				cell.RoadMeshRenderer = roadMeshRenderer;
			}
		}
	}



	public Vector3 GetNearestCellPosition(Vector3 worldPosition)
	{
		int xIndex = Mathf.RoundToInt(worldPosition.x / cellWidth);
		int zIndex = Mathf.RoundToInt(worldPosition.z / cellHeight);

		xIndex = Mathf.Clamp(xIndex, 0, gridWidth - 1);
		zIndex = Mathf.Clamp(zIndex, 0, gridHeight - 1);

		return grid[xIndex][zIndex].Position;
	}

	public bool IsCellEmpty(Vector3 worldPosition)
	{
		int xIndex = Mathf.RoundToInt(worldPosition.x / cellWidth);
		int zIndex = Mathf.RoundToInt(worldPosition.z / cellHeight);

		xIndex = Mathf.Clamp(xIndex, 0, gridWidth - 1);
		zIndex = Mathf.Clamp(zIndex, 0, gridHeight - 1);

		return grid[xIndex][zIndex].Building == null;
	}

    public void PlaceBuilding(GameObject building, Vector3 worldPosition)
	{
		Vector3 cellPosition = GetNearestCellPosition(worldPosition);

		int xIndex = Mathf.RoundToInt(cellPosition.x / cellWidth);
		int zIndex = Mathf.RoundToInt(cellPosition.z / cellHeight);

		grid[xIndex][zIndex].Building = building;
	}

	public void RemoveBuilding(Vector3 worldPosition)
	{
		Vector3 cellPosition = GetNearestCellPosition(worldPosition);

		int xIndex = Mathf.RoundToInt(cellPosition.x / cellWidth);
		int zIndex = Mathf.RoundToInt(cellPosition.z / cellHeight);

		grid[xIndex][zIndex].Building = null;
	}

	public void ChangeZoneType(int x, int z, ZoneType zoneType)
	{
		if (x < 0 || x >= GridWidth || z < 0 || z >= GridHeight) return;

		if (grid[x][z].ZoneType == zoneType) return;

		grid[x][z].ZoneType = zoneType;

		if (zoneType == ZoneType.Road)
		{
			// Check if there is an existing road mesh and destroy it
			if (grid[x][z].RoadMeshRenderer != null)
			{
				Destroy(grid[x][z].RoadMeshRenderer.gameObject);
			}

			// Instantiate the roadMeshPrefab instead of creating a new GameObject
			GameObject roadMeshObject = Instantiate(roadMeshPrefab,grid[x][z].Position, roadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			roadMeshObject.name = "RoadMesh";
			MeshRenderer meshRenderer = roadMeshObject.GetComponent<MeshRenderer>();
			grid[x][z].RoadMeshRenderer = meshRenderer;

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = roadMeshObject;

			UpdateRoadMesh(x, z);
			updateRoadsAround(x, z);
		}
		else
		{
			// Check if the GridCell.Building is a road mesh and destroy it if so
			if (grid[x][z].Building != null && grid[x][z].Building.name == "RoadMesh")
			{
				Destroy(grid[x][z].Building);
				grid[x][z].Building = null;
			}

			
		}

		UpdateCell(x, z);
	}


	public void updateRoadsAround(int x,int z)
	{
		UpdateRoadMesh(x - 1, z);
		UpdateRoadMesh(x + 1, z);
		UpdateRoadMesh(x, z - 1);
		UpdateRoadMesh(x, z + 1);
	}




	public void UpdateCell(int xIndex, int zIndex)
	{
		GridCell cell = grid[xIndex][zIndex];

		GameObject cellObject = cell.CellObject;
		if (cellObject != null)
		{
			Material zoneMaterial = zoneMaterials.GetRandomMaterial(cell.ZoneType);
			MeshRenderer renderer = cellObject.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				renderer.material = zoneMaterial;
			}
		}
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

	Mesh GetRoadMesh(int meshNum)
	{
		//make sure the meshNum is within the range of the roadMeshes array
		if (meshNum < 0 || meshNum >= roadMeshes.Count)
		{
			return null;
		}

		//make a random number between 0 and the roadMeshes[meshNum].lenght
        int randomNum = Random.Range(0, roadMeshes[meshNum].Count);

		//give back a random mesh
		return roadMeshes[meshNum][randomNum];
		

	}

	private void UpdateRoadMesh(int x, int z)
	{
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
			MeshFilter roadMeshFilter = cell.RoadMeshRenderer.GetComponent<MeshFilter>();
			roadMeshFilter.mesh = roadMesh;

			// Set the road mesh rotation
			cell.RoadMeshRenderer.transform.rotation = Quaternion.Euler(-90, rotationY, 0);

			// Enable the road mesh renderer if the zone type is Road
			cell.RoadMeshRenderer.enabled = true;

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
		if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
		{
			return grid[x][z].ZoneType == ZoneType.Road;
		}

		return false;
	}
}

public class GridCell
{
	public Vector3 Position { get; set; }
	public Vector3 Size { get; set; }
	public GameObject Building { get; set; }
	public ZoneType ZoneType { get; set; }
	public GameObject CellObject { get; set; }
	public MeshRenderer RoadMeshRenderer { get; set; }
}
