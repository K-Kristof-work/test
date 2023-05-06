using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Model.Data;
using UnityEngine.UIElements;

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

    void Start()
    {
		zts = GameObject.Find("GameView").GetComponent<ZoneTypeSelector>();
		gameView = GameObject.Find("GameView").GetComponent<GameView>();
        floatingObject = null;

		BuildingPrefabs = new Dictionary<BlockType, GameObject>
		{
			{BlockType.PoliceStation,  PoliceFloatingObject},
			{BlockType.University, UniversityFloatingObject},
			{BlockType.Stadium, StadiumFloatingObject}
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
				floatingObject.transform.position = mousePosition;
			}
		}

		// if clicked, destroy the floating object
		if (Input.GetMouseButtonDown(0))
		{

			/*
			 *		RaycastHit hit;
					Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

					if (Physics.Raycast(ray, out hit))
					{
						gameView.placebuilding(hit.point, blockType);
						return;
					}
			 * 
			*/

			Destroy(floatingObject);
			floatingObject = null;


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
            gameView.HandleDebug(this, "ERROR: BlockType not found");
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
