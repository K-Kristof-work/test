using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Assets.Model.Data;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
	#region public variables

	public List<List<GridCell>> grid = new List<List<GridCell>>();
	public float cellWidth; // Set the desired cell width.
	public float cellHeight; // Set the desired cell height.
	public int gridWidth; // Set the desired grid width.
	public int gridHeight; // Set the desired grid height.
	public GameObject cellPrefab;
	public Transform gridParent; // Drag and drop the "GameView" GameObject to this field in the Inspector
	public GameObject roadMeshPrefab;
	public GameObject IncomingRoadMeshPrefab;
	public GameObject WaterMeshPrefab;

    private List<(string, int)> budgetTracking = new();

    //make list for all of the road meshes
    public List<Mesh> StraightRoad = new List<Mesh>();
	public List<Mesh> CornerRoad = new List<Mesh>();
	public List<Mesh> TIntersectionRoad = new List<Mesh>();
	public List<Mesh> CrossRoad = new List<Mesh>();

	public Dictionary<int, List<Mesh>> roadMeshes;

	public float buildInterval = 0.5f;
	public List<GameObject> residentialBuildingPrefabs;
	public List<GameObject> commercialBuildingPrefabs;
	public List<GameObject> industrialBuildingPrefabs;
	public List<GameObject> policeBuildingPrefabs;
	public List<GameObject> StadiumBuildingPrefabs;
	public List<GameObject> SchoolBuildingPrefabs;
	public List<GameObject> UniversityBuildingPrefabs;

    public TextMeshProUGUI UI_Time;
    public TextMeshProUGUI UI_Money;
    public TextMeshProUGUI UI_Happiness;
	public TextMeshProUGUI UI_Population;

	public GameObject HappinessIcon;

    public GameObject ExpenseContent;
    public GameObject ExpenseRow;
    public GameObject IncomeRow;
    public int ExpenseRowCount = 0;

    public GameObject SpeedObject;

    public Sprite PauseButtonImage;
    public Sprite SlowButtonImage;
    public Sprite MediumButtonImage;
    public Sprite FastButtonImage;

    public Sprite HappyFace;
    public Sprite NeutralFace;
    public Sprite SadFace;

    public GameObject FloatingObject;

	public List<string> debugBanList = new List<string>();

	#endregion

	#region private variables

	private Dictionary<BlockType, List<GameObject>> zoneTypeToBuildingPrefabs;

	//public List<List<Mesh>>  roadMeshes;
	private ZoneMaterials zoneMaterials;
	private bool isroadupdating = false;
	private GameData gameData;

	

	#endregion

	#region public properties
	public int GridWidth { get { return gridWidth; } }
	public int GridHeight { get { return gridHeight; } }
	public float CellWidth { get { return cellWidth; } }
	public float CellHeight { get { return cellHeight; } }

	#endregion

	#region unity methods


	public void HandleDebug(string message)
	{
		Debug.Log(message);
	}

    public void PlayButtonPressed()
    {
        gameData.cityLogic.SpeedChange();
        ChangeSpeedIcon();
    }

    public void ChangeSpeedIcon()
    {
        switch (gameData.time.speed)
        {
            case 0:
                SpeedObject.GetComponent<Image>().sprite = PauseButtonImage;
                break;
            case 1:
                SpeedObject.GetComponent<Image>().sprite = SlowButtonImage;
                break;
            case 2:
                SpeedObject.GetComponent<Image>().sprite = MediumButtonImage;
                break;
            case 3:
                SpeedObject.GetComponent<Image>().sprite = FastButtonImage;
                break;
        }
    }

    void Start()
	{
		roadMeshes = new Dictionary<int, List<Mesh>>
		{
			{0, StraightRoad},
			{1, CornerRoad},
			{2, TIntersectionRoad},
			{3, CrossRoad},
		};

		zoneTypeToBuildingPrefabs = new Dictionary<BlockType, List<GameObject>>
		{
			{BlockType.House,  residentialBuildingPrefabs},
			{BlockType.Shop, commercialBuildingPrefabs},
			{BlockType.Factory, industrialBuildingPrefabs},
			{BlockType.PoliceStation, policeBuildingPrefabs},
			{BlockType.Stadium, StadiumBuildingPrefabs},
			{BlockType.School, SchoolBuildingPrefabs},
			{BlockType.University, UniversityBuildingPrefabs},

		};

		gameData = GameModel.instance.gameData;

		gameData.OnZoneTypeChanged += HandleZoneTypeChanged;
		gameData.OnBuildingPlaced += HandleBuildingPlaced;
        gameData.cityLogic.OnTimeChanged += HandleTime;
        gameData.cityLogic.OnMoneyChanged += HandleMoney;
		gameData.cityLogic.OnHappinessChanged += HandleHappiness;
		gameData.cityLogic.OnCitizenChanged += HandlePopulation;
        gameData.OnDebug += HandleDebug;

		UnityThread.initUnityThread();

		zoneMaterials = GetComponent<ZoneMaterials>();

		SetUpGrid(gridWidth, gridHeight);
		gameData.SetUpGrid(gridWidth, gridHeight);

		ChangeSpeedIcon();
		//StartCoroutine(PlaceBuildingsOverTime());

	}


	private void HandlePopulation(int count)
	{
        UnityThread.executeInUpdate(() =>
        {
            UI_Population.text = count.ToString();
        });
    }

    private void HandleTime(Assets.Model.Data.Time time)
    {
        UnityThread.executeInUpdate(() =>
        {
            UI_Time.text = time.date.Hour + ":" + time.date.Minute + "\n" + time.date.Year.ToString() + ". " + time.date.Month + ". " + time.date.Day + ".";
        });
    }

    private void HandleMoney(int balance, int difference, string type)
    {
		//UPDATE THE UI
        UnityThread.executeInUpdate(() =>
        {
			if (difference == 0) return;

            difference *= -1;
            budgetTracking.Add((type, difference));

            UI_Money.text = balance.ToString();

			//Create a new row
			GameObject newRow;

            if (difference < 0)
			{
                newRow = Instantiate(ExpenseRow);
			}
			else
			{
                newRow = Instantiate(IncomeRow);
            }
           
            //change the rows rect transform y position
            newRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(400, ExpenseRowCount * -50 + 25);
            ExpenseRowCount++;
            TextMeshProUGUI typeText = newRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI diffrenceText = newRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            typeText.text = type;
            diffrenceText.text = difference.ToString();

            //set the row to be a child of the content
            newRow.transform.SetParent(ExpenseContent.transform, false);
            Debug.Log(typeText.text);

            // update height
            RectTransform rt = ExpenseContent.GetComponent(typeof(RectTransform)) as RectTransform;
			rt.sizeDelta = new Vector2(0,rt.sizeDelta.y + 50);

        });
    }

	private void HandleHappiness(double happiness)
	{
        Debug.Log("Happiness: " + happiness);
        happiness *= 100;
        UI_Happiness.text = happiness.ToString("0.00") + "%";
        UnityThread.executeInUpdate(() =>
        {
            UI_Happiness.text = happiness.ToString("0.00") + "%";
            if (happiness > 70)
            {
                HappinessIcon.GetComponent<Image>().sprite = HappyFace;
            }
            else if (happiness > 40)
            {
                HappinessIcon.GetComponent<Image>().sprite = NeutralFace;
            }
            else
            {
                HappinessIcon.GetComponent<Image>().sprite = SadFace;
            }
        });
    }

	private void OnApplicationQuit()
	{
		gameData.OnApplicationExit();
		gameData.OnZoneTypeChanged -= HandleZoneTypeChanged;
		gameData.OnBuildingPlaced -= HandleBuildingPlaced;
		gameData.OnDebug -= HandleDebug;
        gameData.cityLogic.OnTimeChanged -= HandleTime;
        gameData.cityLogic.OnMoneyChanged -= HandleMoney;
		gameData.cityLogic.OnHappinessChanged -= HandleHappiness;
    }

	#endregion

	#region methods for grid creation and zone managment

	#region grid creation
	public void SetUpGrid(int _gridWidth, int _gridHeight)
	{

		for (int i = 0; i < _gridWidth; i++)
		{
			grid.Add(new List<GridCell>());

			for (int j = 0; j < _gridHeight; j++)
			{
				GridCell cell = new GridCell
				{
					Position = new Vector3(i * cellWidth, 0, j * cellHeight),
					Size = new Vector3(cellWidth / 10, cellWidth / 10, cellHeight / 10),
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
				Material zoneMaterial = zoneMaterials.GetRandomMaterial(ZoneType.Empty);
				MeshRenderer renderer = cellInstance.GetComponent<MeshRenderer>();
				if (renderer != null)
				{
					renderer.material = zoneMaterial;
				}

			}
		}


	}
	#endregion

	#region zone managment

	public void HandleZoneTypeChanged(int x, int z, ZoneType zoneType)
	{
		if (!CheckGrid(x, z)) return;

		if (zoneType == ZoneType.IncomingRoad)
		{

			// Instantiate the roadMeshPrefab
			GameObject roadMeshObject = Instantiate(IncomingRoadMeshPrefab, grid[x][z].Position, IncomingRoadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			roadMeshObject.name = "IncomingRoadMesh";

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = roadMeshObject;
			if (z == 0 || z == gridWidth - 1)
			{
				grid[x][z].Building.transform.rotation = Quaternion.Euler(-90,
																		  grid[x][z].Building.transform.rotation.y + 90,
																		  grid[x][z].Building.transform.rotation.z);
			}

			UpdateCell(x, z, zoneType);
		}

		if (zoneType == ZoneType.Water)
		{
			// Instantiate the roadMeshPrefab
			GameObject WaterObject = Instantiate(WaterMeshPrefab, grid[x][z].Position, IncomingRoadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			WaterObject.name = "WaterMesh";

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = WaterObject;

			//set the rotation
			SetWaterRotation(x, z);
			UpdateCell(x, z, zoneType);
		}


		if (zoneType == ZoneType.Road)
		{
			// Instantiate the roadMeshPrefab
			GameObject roadMeshObject = Instantiate(roadMeshPrefab, grid[x][z].Position, roadMeshPrefab.transform.rotation, grid[x][z].CellObject.transform);
			roadMeshObject.name = "RoadMesh";

			// Store the road mesh game object in the GridCell.Building variable
			grid[x][z].Building = roadMeshObject;

			HandleDebug(this, " RoadMesh created");

			UpdateRoadMesh(x, z);
			updateRoadsAround(x, z);
		}

		if(zoneType == ZoneType.Empty && grid[x][z].Building != null)
        {
			Destroy(grid[x][z].Building);
			grid[x][z].Building = null;

		}



		UpdateCell(x, z, zoneType);
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
			if (gameData.grid[x][z + 1].zone is not null && gameData.grid[x][z + 1].zone.zone_type == ZoneType.IncomingRoad)
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
			if (gameData.grid[x + 1][z].zone is not null && gameData.grid[x + 1][z].zone.zone_type == ZoneType.IncomingRoad)
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
			if (gameData.grid[x + 1][z].zone is not null && gameData.grid[x + 1][z].zone.zone_type == ZoneType.IncomingRoad)
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
			if (gameData.grid[x][z + 1].zone is not null && gameData.grid[x][z + 1].zone.zone_type == ZoneType.IncomingRoad)
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

	public void UpdateCell(int xIndex, int zIndex, ZoneType zonetype)
	{
		GridCell cell = grid[xIndex][zIndex];

		GameObject cellObject = cell.CellObject;
		if (cellObject != null)
		{

			Material zoneMaterial = zoneMaterials.GetRandomMaterial(zonetype);

			MeshRenderer renderer = cellObject.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				renderer.material = zoneMaterial;
			}
		}
	}

	public void SetZone(Vector3 worldPosition, ZoneType SelectedZoneType)
	{
		int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / CellWidth + CellWidth / 2);
		int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / CellHeight + CellHeight / 2);
		gameData.ChangeZoneType(x, z, SelectedZoneType, gameData.getNextZoneId());

	}

	public void SetZoneRectangle(Vector3 start, Vector3 end, ZoneType SelectedZoneType)
	{
		float halfCellWidth = CellWidth * 0.5f;
		float halfCellHeight = CellHeight * 0.5f;
		int startX = Mathf.FloorToInt(Mathf.Min(start.x + halfCellWidth, end.x + halfCellWidth));
		int startZ = Mathf.FloorToInt(Mathf.Min(start.z + halfCellHeight, end.z + halfCellHeight));
		int endX = Mathf.FloorToInt(Mathf.Max(start.x + halfCellWidth, end.x + halfCellWidth));
		int endZ = Mathf.FloorToInt(Mathf.Max(start.z + halfCellHeight, end.z + halfCellHeight));

		int zoneId = gameData.getNextZoneId();

		gameData.ChangeZoneTypeRectangle(new Vec2((uint)startX, (uint)startZ), new Vec2((uint)endX, (uint)endZ), SelectedZoneType, zoneId);
	}

	public void OnCellClicked(int xIndex, int zIndex, ZoneType newZoneType)
	{
		gameData.ChangeZoneType(xIndex, zIndex, newZoneType, gameData.getNextZoneId());
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

		if (gameData.grid[x][z].zone.zone_type == ZoneType.Road && !isroadupdating)
		{
			isroadupdating = true;
			updateRoadsAround(x, z);

			if (x == 10 && z == 10)
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

			//write all of the cell data out with the handledebug
			HandleDebug(this,"x: " + x + " z: " + z + " roadMask: " + roadMask + " rotationY: " + rotationY);

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
			return (gameData.grid[x][z].zone.zone_type == ZoneType.Road || gameData.grid[x][z].zone.zone_type == ZoneType.IncomingRoad);
		}

		return false;
	}

	#endregion

	#endregion

	#region methods for buildings

	private void HandleBuildingPlaced(List<Vec2> positions, Block block)
	{
		HandleDebug(this, "building placement on main thread starting");
		UnityThread.executeInUpdate(() =>
		
		{
			Placebuilding(positions, block);
		});
	}

	private void Placebuilding(List<Vec2> positions, Block block)
	{

		HandleDebug(this, "building placment starting in view");

		int x = (int)positions[0].x;
		int z = (int)positions[0].y;

		// Get a random building prefab of the appropriate size for the zone type
		List<GameObject> suitablePrefabs = zoneTypeToBuildingPrefabs[block.type].Where(prefab =>
		{
			BuildingPrefab prefabScript = prefab.GetComponent<BuildingPrefab>();
			return (prefabScript.BuildingSize.x == block.blockSize.x && prefabScript.BuildingSize.y == block.blockSize.y) ||
				   (prefabScript.BuildingSize.y == block.blockSize.x && prefabScript.BuildingSize.x == block.blockSize.y);
		}).ToList();
		if (suitablePrefabs.Count == 0)
		{
			HandleDebug(this, "no suitable prefab found");
			return;
		}
		GameObject buildingPrefab = suitablePrefabs[Random.Range(0, suitablePrefabs.Count)];

		HandleDebug(this, "random prefab selected");

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

		HandleDebug(this, "prefab looks at the road");

		float buildingPosY = buildingPrefab.transform.position.y;
		int chosenQuadrant = -1;
		Vector3 centerPosition = GetCenterPosition(positions, buildingPosY, ref chosenQuadrant);

		HandleDebug(this, "prefab center position calculated");

		GameObject buildingInstance = Instantiate(buildingPrefab, centerPosition, buildingRotation, grid[x][z].CellObject.transform);
		grid[x][z].Building = buildingInstance;
		grid[x][z].Building.GetComponent<BuildingPrefab>().BuildingSize.x = (int)block.blockSize.x;
		grid[x][z].Building.GetComponent<BuildingPrefab>().BuildingSize.y = (int)block.blockSize.y;

		HandleDebug(this, "prefab added to grid");

		// Update the other grid cells that the building spawns on top of

		foreach (Vec2 position in positions)
		{
			int affectedX = (int)position.x;
			int affectedZ = (int)position.y;
			if (affectedX != x || affectedZ != z)
			{
				grid[affectedX][affectedZ].Building = buildingInstance;
				HandleDebug(this, "cell  in position " + x + ", " + z + " have been allocated for the building");
			}
		}

		HandleDebug(this, "other cells have been allocated for the building");

		// Attaching the BuildingAnimationController script to the building will play the animation

		if(block.type == BlockType.House || block.type == BlockType.Factory || block.type == BlockType.Shop)
		buildingInstance.AddComponent<BuildAnimationController>();

		HandleDebug(this, "placeing builidng is finished");
	}

	private int GetRoadDirection(int x, int z)
	{
		int[] dx = { 1, 0, -1, 0 };
		int[] dz = { 0, 1, 0, -1 };

		for (int dir = 0; dir < 4; dir++)
		{
			int newX = x + dx[dir];
			int newZ = z + dz[dir];

			if (newX >= 0 && newZ >= 0 && newX < GridWidth && newZ < GridHeight && gameData.grid[newX][newZ].zone.zone_type == ZoneType.Road)
			{
				return dir;
			}
		}

		return -1;
	}


	private Vector3 GetCenterPosition(List<Vec2> positions, float buildingPosY, ref int chosenQuadrant)
	{
		float sumX = 0;
		float sumZ = 0;

		foreach (Vec2 position in positions)
		{
			sumX += position.x;
			sumZ += position.y;
		}

		float avgX = sumX / positions.Count;
		float avgZ = sumZ / positions.Count;

		// Get y coordinate (height) from the first position in the list
		int firstX = (int)positions[0].x;
		int firstZ = (int)positions[0].y;

		return new Vector3(avgX, buildingPosY/10f, avgZ);
	}


	public void PlaceBuildingByUser(Vector3 worldPosition, BlockType type)
	{
		//convert world position to grid position
		int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / CellWidth + CellWidth / 2);
		int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / CellHeight + CellHeight / 2);
		if (!CheckGrid(x, z)) return;
		Vec2 pos = new Vec2((uint)x, (uint)z);

		gameData.buildingPlacer.PlaceBuildingByUser(pos, type);

		//place the building there
		

	}

	#endregion



	public void HandleDebug(object ob, string message)
	{
		
		//check if ob is in debugbanlist
		if (debugBanList.Contains(ob.GetType().Name))
		{
			return;
		}

		Debug.Log(ob.GetType().Name + ": " + message);
	}


    //print out gamedata.grid to console
    public void PrintGrid()
	{
		string gridString = "";
		for (int i = 0; i < GridWidth; i++)
		{
			for (int j = 0; j < GridHeight; j++)
			{
				if(gameData.grid[i][j].block == null)
					gridString += "- ";
				else
				{
					gridString += gameData.grid[i][j].block.type.ToString()[0] + " ";
				}
			}
			gridString += "\n";
		}
		Debug.Log(gridString);
	}

	public Vec2 GetBuildingPlacerSizeForBuildingType(BlockType type)
	{
			return gameData.buildingPlacer.GetSizeForBuildingType(type);
	}

}

public class GridCell
{
	#region variables

	public Vector3 Position { get; set; }
	public Vector3 Size { get; set; }
	public GameObject Building { get; set; }
	public GameObject CellObject { get; set; }
	public bool IsConnectedToIncomingRoad = false;

	#endregion
}

