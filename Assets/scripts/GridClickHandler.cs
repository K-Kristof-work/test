using Assets.Model.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridClickHandler : MonoBehaviour
{
	private Camera mainCamera;
	private bool isDragging = false;
	private Vector3 dragStartWorldPosition;
	private Vector3 dragEndWorldPosition;
	[HideInInspector]
	public bool ZoneButtonSelected = false;
	
	public ZoneType SelectedZoneType { get; set; }
	public GameView gameView; // Drag and drop the GridManager object to this field in the Inspector

	public SelectionBox selectionBox; // Drag and drop the Image GameObject to this field in the Inspector



	void Start()
	{
		mainCamera = Camera.main;
	}

	void Update()
	{
		if (ZoneButtonSelected)
		{
			if (EventSystem.current.IsPointerOverGameObject()) return;

			if (SelectedZoneType == ZoneType.Road)
			{
				if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) // Left mouse button down or held down
				{
					RaycastHit hit;
					Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

					if (Physics.Raycast(ray, out hit))
					{
						gameView.SetZone(hit.point, SelectedZoneType);
						return;
					}
				}
			}

			if (Input.GetMouseButtonDown(0)) // Left mouse button down
			{
				RaycastHit hit;
				Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					dragStartWorldPosition = hit.point;
					isDragging = true;
					selectionBox.SetVisible(true);
				}
			}
			else if (Input.GetMouseButtonUp(0) && isDragging) // Left mouse button released
			{
				RaycastHit hit;
				Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					dragEndWorldPosition = hit.point;
					gameView.SetZoneRectangle(dragStartWorldPosition, dragEndWorldPosition, SelectedZoneType);
				}
				isDragging = false;
				selectionBox.SetVisible(false);
				GetComponent<ZoneTypeSelector>().DisableTypeSelector();
			}

			if (isDragging)
			{
				RaycastHit hit;
				Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					Vector3 endWorldPosition = hit.point;
					selectionBox.UpdateSelectionBox(dragStartWorldPosition, endWorldPosition);
				}
			}
		}
		
	}


	



}
