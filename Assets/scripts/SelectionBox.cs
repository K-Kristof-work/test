using UnityEngine;

public class SelectionBox : MonoBehaviour
{
	private LineRenderer lineRenderer;
	private Vector3[] corners = new Vector3[5];

	void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	public void SetVisible(bool visible)
	{
		lineRenderer.enabled = visible;
	}

	public bool IsVisible()
    {
		return lineRenderer.enabled;
	}

	public void UpdateSelectionBox(Vector3 start, Vector3 end)
	{
		corners[0] = new Vector3(start.x, start.y, start.z);
		corners[1] = new Vector3(start.x, start.y, end.z);
		corners[2] = new Vector3(end.x, end.y, end.z);
		corners[3] = new Vector3(end.x, start.y, start.z);
		corners[4] = new Vector3(start.x, start.y, start.z);

		lineRenderer.positionCount = corners.Length;
		lineRenderer.SetPositions(corners);
	}
}
