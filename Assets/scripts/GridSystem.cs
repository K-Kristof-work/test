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
	private ZoneMaterials zoneMaterials;

	public int GridWidth { get { return gridWidth; } }
	public int GridHeight { get { return gridHeight; } }
	public float CellWidth { get { return cellWidth; } }
	public float CellHeight { get { return cellHeight;   } }
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
					Size = new Vector3(cellWidth/10, 0.01f, cellHeight/10),
					ZoneType = ZoneType.Empty // Set the default zone type
				};

				grid[i].Add(cell);

				// Instantiate the cell prefab

				GameObject cellInstance = Instantiate(cellPrefab, cell.Position, Quaternion.identity, gridParent);
				// get this zone material component
				ZoneMaterials zoneMaterials = GetComponent<ZoneMaterials>();
				cellInstance.transform.localScale = cell.Size;
				cellInstance.name = $"GridCell_{i}_{j}"; // Set the cell name with indices

				// Store the instantiated GameObject in the GridCell
				cell.CellObject = cellInstance;

				// Assign the correct material based on the zone type
				Material zoneMaterial = zoneMaterials.GetRandomMaterial(cell.ZoneType);
				MeshRenderer renderer = cellInstance.GetComponent<MeshRenderer>();
				if (renderer != null)
				{
					renderer.material = zoneMaterial;
				}
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

	public void ChangeZoneType(int xIndex, int zIndex, ZoneType newZoneType)
	{
		GridCell cell = grid[xIndex][zIndex];
		cell.ZoneType = newZoneType;

		GameObject cellObject = cell.CellObject;
		if (cellObject != null)
		{
			Material zoneMaterial = zoneMaterials.GetRandomMaterial(newZoneType);
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
		Debug.Log($"Cell clicked: {xIndex}, {zIndex}");
	}


}

public class GridCell
{
	public Vector3 Position { get; set; }
	public Vector3 Size { get; set; }
	public GameObject Building { get; set; }
	public ZoneType ZoneType { get; set; }
	public GameObject CellObject { get; set; }
}
