using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildAnimationController : MonoBehaviour
{
    public int numSphereMasks = 10; // The number of SphereMasks to create
    public GameObject maskContainerPrefab; // The prefab for the MaskContainer object
    public GameObject sphereMaskPrefab; // The prefab for the SphereMask object
    public GameObject squareMaskPrefab; // The prefab for the SquareMask object

    private int buildingX;
    private int buildingY;

    private float cellX;
    private float cellY;

    private GameObject maskContainer; // The instance of the MaskContainer object created at runtime

    void Start()
    {
        //Get the script from the game object
        var BuildingPrefab = gameObject.GetComponent<BuildingPrefab>();
        //Set the variables to the values from the script
        buildingX = BuildingPrefab.BuildingSize.x;
        buildingY = BuildingPrefab.BuildingSize.y;
        //Find the GridSystem script
        GridSystem gridSystem = GameObject.Find("MyObject").GetComponent<GridSystem>();
        //Set the variables to the values from the script
        cellX = gridSystem.cellHeight;
        cellY = gridSystem.cellWidth;
        // Instantiate the MaskContainer object from the prefab
        maskContainer = Instantiate(maskContainerPrefab, transform.position, Quaternion.identity);
        GameObject squareMask = Instantiate(squareMaskPrefab, transform.position, Quaternion.identity);
        squareMask.transform.SetParent(maskContainer.transform);
        // Fill the MaskContainer with SphereMask objects
        for (int i = 0; i < numSphereMasks; i++)
        {
            // Instantiate the SphereMask object from the prefab
            GameObject sphereMask = Instantiate(sphereMaskPrefab, maskContainer.transform);
            maskContainer.transform.SetParent(maskContainer.transform);
        }
        StartCoroutine(MoveContainerUpward());
    }

    IEnumerator MoveContainerUpward()
    {
        float duration = 2.0f; // The duration of the upward movement
        float elapsed = 0.0f; // The elapsed time

        Vector3 startPos = maskContainer.transform.position; // The starting position
        Vector3 endPos = maskContainer.transform.position + Vector3.up * 5.0f; // The ending position

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration); // The normalized time

            maskContainer.transform.position = Vector3.Lerp(startPos, endPos, t); // Interpolate the position

            yield return null; // Wait for the next frame
        }
    }
}
