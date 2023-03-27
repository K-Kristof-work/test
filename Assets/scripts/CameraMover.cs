using UnityEngine;

public class CameraMover : MonoBehaviour
{
	public float dragSpeed = 10f;
	public float extraMotionFactor = 2f;
	public float zoomSpeed = 10f;
	public float minZoom = 0f;
	public float maxZoom = 18f;
	public float rotationSpeed = 3f;
	public float minXRotation = 20f;
	public float maxXRotation = 80f;

	private Vector3 dragOrigin;
	private float currentZoom;

	void Start()
	{
		currentZoom = Mathf.Abs(transform.position.y);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			dragOrigin = Input.mousePosition;
			return;
		}

		if (Input.GetMouseButton(1))
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				RotateCamera();
			}
			else
			{
				MoveCamera();
			}
		}

		ZoomCamera();
	}

	private void MoveCamera()
	{
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
		Vector3 moveDirection = new Vector3(-pos.x * (dragSpeed + extraMotionFactor), 0, -pos.y * (dragSpeed + extraMotionFactor));
		Vector3 move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * moveDirection;

		transform.Translate(move, Space.World);
		dragOrigin = Input.mousePosition;
	}

	private void RotateCamera()
	{
		Vector3 currentEulerAngles = transform.eulerAngles;
		float mouseX = -Input.GetAxis("Mouse X") * rotationSpeed;
		float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

		float newAngleX = currentEulerAngles.x + mouseY;
		if (newAngleX > 180) newAngleX -= 360; // Convert angle to a value between -180 and 180

		newAngleX = Mathf.Clamp(newAngleX, minXRotation, maxXRotation);

		transform.rotation = Quaternion.Euler(newAngleX, currentEulerAngles.y - mouseX, currentEulerAngles.z);
	}


	private void ZoomCamera()
	{
		float scrollInput = Input.GetAxis("Mouse ScrollWheel");
		if (scrollInput != 0)
		{
			Vector3 zoomDirection = transform.rotation * Vector3.forward;
			Vector3 newPosition = transform.position + zoomDirection * (scrollInput * zoomSpeed);
			float newZoom = currentZoom + scrollInput * zoomSpeed;

			if (newZoom >= minZoom && newZoom <= maxZoom)
			{
				transform.position = newPosition;
				currentZoom = newZoom;
			}
		}
	}
}
