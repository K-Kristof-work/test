using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildAnimationController : MonoBehaviour
{
    // The amount to move the object downward when the script starts
    public float fallDistance = 5.0f;

    // The speed at which the object returns to its original position
    public float returnSpeed = 0.5f;

    // The original position of the object
    private Vector3 originalPosition;

    // A flag to indicate whether the object is falling or returning
    private bool isFalling = true;

    void Start()
    {
        // Store the object's original position
        originalPosition = transform.position;

        // Move the object downward by the specified distance
        transform.position -= Vector3.up * fallDistance;
    }

    void Update()
    {
        if (isFalling)
        {
            // If the object is falling, check if it has reached its target position
            if (transform.position.y >= originalPosition.y)
            {
                // If the object has reached its target position, stop falling
                isFalling = false;
            }
            else
            {
                // If the object hasn't reached its target position, continue falling
                transform.position += Vector3.up * returnSpeed * Time.deltaTime;
            }
        }
    }

    /*public int numSphereMasks; // The number of SphereMasks to create
    private GameObject maskContainerPrefab; // The prefab for the MaskContainer object
    private GameObject sphereMaskPrefab; // The prefab for the SphereMask object
    private GameObject boxMaskPrefab; // The prefab for the BoxMask object

    private float maxRadius;
    private float minRadius;

    private float maxSpeed;
    private float minSpeed;

    private float animationTime;
    private float animationSpeed;

    private int buildingX;
    private int buildingY;

    private float cellX;
    private float cellY;

    private float boxHeight;

    private GameObject maskContainer; // The instance of the MaskContainer object created at runtime

    void Start()
    {
        // Find a gameObejct in the scene with the name "AnimationController"
        GameObject animationController = GameObject.Find("AnimationController");
        var animationControllerScript = animationController.GetComponent<AnimationController>();
        // Set the variables to the values from the script
        maskContainerPrefab = animationControllerScript.maskContainer;
        sphereMaskPrefab = animationControllerScript.sphereMask;
        boxMaskPrefab = animationControllerScript.cubeMask;
        numSphereMasks = animationControllerScript.howManyMaskSpheres;
        boxHeight = animationControllerScript.boxHeight;
        maxRadius = animationControllerScript.maxRadius;
        minRadius = animationControllerScript.minRadius;
        maxSpeed = animationControllerScript.maxSpeed;
        minSpeed = animationControllerScript.minSpeed;
        animationTime = animationControllerScript.animationTime;
        animationSpeed = animationControllerScript.animationSpeed;
        // Get the script from the game object
        var BuildingPrefab = gameObject.GetComponent<BuildingPrefab>();
        // Set the variables to the values from the script
        buildingX = BuildingPrefab.BuildingSize.x;
        buildingY = BuildingPrefab.BuildingSize.y;
        // Find the GridSystem script
        GridSystem gridSystem = GameObject.Find("Grid System").GetComponent<GridSystem>();
        // Set the variables to the values from the script
        cellX = gridSystem.cellHeight;
        cellY = gridSystem.cellWidth;
        // Instantiate the MaskContainer object from the prefab
        maskContainer = Instantiate(maskContainerPrefab, transform.position, Quaternion.identity);
        // Creating the sphere masks
        for (int i = 0; i < numSphereMasks * buildingX * buildingY; i++)
        {
            GameObject sphereMask = Instantiate(sphereMaskPrefab, maskContainer.transform.position, Quaternion.identity);
            sphereMask.GetComponent<MaskObject>().objectToMask = gameObject;
            sphereMask.transform.SetParent(maskContainer.transform);
            sphereMask.transform.localScale = new Vector3(Random.Range(minRadius, maxRadius) * buildingX * buildingY, Random.Range(minRadius, maxRadius) * buildingX * buildingY, Random.Range(minRadius, maxRadius) * buildingX * buildingY);
            // Set the radius of the sphere mask
            SphereCollider sphereCollider = sphereMask.GetComponent<SphereCollider>();
            sphereCollider.radius = Random.Range(minRadius, maxRadius)*buildingX*buildingY;
        }
        // Add a boxMask to the MaskContainer object
        GameObject cubeMask = Instantiate(boxMaskPrefab, maskContainer.transform.position, Quaternion.identity);
        cubeMask.GetComponent<MaskObject>().objectToMask = gameObject;
        cubeMask.transform.SetParent(maskContainer.transform);
        // Set the size of the box collider of the cube mask
        BoxCollider boxCollider = cubeMask.GetComponent<BoxCollider>();
        cubeMask.transform.localScale = new Vector3(buildingX * cellX, boxHeight, buildingY * cellY);
        // Get the child object's current local position
        Vector3 localPosition = cubeMask.transform.localPosition;
        // Add 5 units to the Y value
        localPosition.y += boxHeight/2;
        // Set the child object's new local position
        cubeMask.transform.localPosition = localPosition;

        StartCoroutine(MoveContainerUpward());
    }

    IEnumerator MoveContainerUpward()
    {
        float duration = animationTime; // The duration of the upward movement
        float elapsed = 0.0f; // The elapsed time

        Vector3 startPos = maskContainer.transform.position; // The starting position
        Vector3 endPos = maskContainer.transform.position + Vector3.up * animationSpeed; // The ending position

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration); // The normalized time

            maskContainer.transform.position = Vector3.Lerp(startPos, endPos, t); // Interpolate the position

            yield return null; // Wait for the next frame
        }

        Destroy(maskContainer); // Destroy the MaskContainer object
    }*/
}
