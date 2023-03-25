using UnityEngine;

public class GridClickHandler : MonoBehaviour
{
	private Camera mainCamera;
	private bool isDragging = false;
	private Vector3 dragStartWorldPosition;
	private Vector3 dragEndWorldPosition;

	public ZoneType SelectedZoneType { get; set; } = ZoneType.Residential; // Set the default selected zone type
	public GridSystem gridSystem; // Drag and drop the GridManager object to this field in the Inspector

	public SelectionBox selectionBox; // Drag and drop the Image GameObject to this field in the Inspector



	void Start()
	{
		mainCamera = Camera.main;
	}

	void Update()
	{
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
				isDragging = false;
				selectionBox.SetVisible(false);
				gridSystem.SetZoneRectangle(dragStartWorldPosition, dragEndWorldPosition, SelectedZoneType);
			}
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
