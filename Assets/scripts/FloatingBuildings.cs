using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Model.Data;
using UnityEngine.UIElements;
using System;

public class FloatingBuildings : MonoBehaviour
{
	private Dictionary<BlockType, GameObject> BuildingPrefabs;
	private GameView gameView;
    private GameObject floatingObject;
	private BlockType blockType;
	private ZoneTypeSelector zts;

    public GameObject PoliceFloatingObject;
    public GameObject UniversityFloatingObject;
    public GameObject HighschoolFloatingObject;
    public GameObject StadiumFloatingObject;
    public GameObject ForestFloatingObject;

    void Start()
    {
		zts = GameObject.Find("GameView").GetComponent<ZoneTypeSelector>();
		gameView = GameObject.Find("GameView").GetComponent<GameView>();
        floatingObject = null;

		BuildingPrefabs = new Dictionary<BlockType, GameObject>
		{
			{BlockType.PoliceStation,  PoliceFloatingObject},
			{BlockType.University, UniversityFloatingObject},
			{BlockType.Stadium, StadiumFloatingObject},
			{BlockType.School, HighschoolFloatingObject},
			{BlockType.Forest, ForestFloatingObject }
		};
	}

	void Update()
	{
		// the floating object follows the mouse
		if (floatingObject != null)
		{
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			Plane gridPlane = new Plane(Vector3.up, Vector3.zero);

			float rayDistance;
			if (gridPlane.Raycast(mouseRay, out rayDistance))
			{
				Vector3 mousePosition = mouseRay.GetPoint(rayDistance);
				Vec2 offset = gameView.GetBuildingPlacerSizeForBuildingType(blockType);
				if (offset.x == 2 || offset.y == 2)
				{
					mousePosition.x += 0.5f;
					mousePosition.z += 0.5f;
				}
				gameView.HandleDebug(this, mousePosition.ToString());
				// Adjust the Y position by half of the prefab's height
				if(floatingObject.GetComponent<MeshRenderer>() != null)
				{
                    float prefabHeight = floatingObject.GetComponent<MeshRenderer>().bounds.size.y;
                    mousePosition.y += prefabHeight / 2;
                }

				floatingObject.transform.position = mousePosition;
			}


			// if clicked, destroy the floating object
			if (Input.GetMouseButtonDown(0))
			{
				Destroy(floatingObject);
				floatingObject = null;

				RaycastHit hit;
				Camera mainCamera = Camera.main;
				Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					gameView.PlaceBuildingByUser(hit.point, blockType);
				}

			}
		}

		
	}


	public void OnBuildingPlacerButtonClicked(string type)
	{
		zts.DisableTypeSelector();

        if (floatingObject != null) return;

        //find type in the blocktypes enum list
        blockType = BlockType.Empty;
        foreach (BlockType bt in System.Enum.GetValues(typeof(BlockType)))
        {
			if (bt.ToString() == type)
            {
				blockType = bt;
				break;
			}
		}

        if(blockType== BlockType.Empty)
        {
            //throw error
            gameView.HandleDebug(this, "BlockType not found");
            return;
        }

		GameObject prefab = BuildingPrefabs[blockType];

        //initialize the building at the mouse position
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;
		prefab.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

		//add the building to the scene
		floatingObject = Instantiate(prefab);


	}
}
