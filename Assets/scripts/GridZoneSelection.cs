using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Model.Data;

public class GridZoneSelection : MonoBehaviour
{
    // Start is called before the first frame update

    private Camera mainCamera;
    private GameData gameData;
    private GameView gameView;

    void Start()
    {
        gameData = GameModel.instance.gameData;
        gameView = GetComponent<GameView>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) // Left mouse button down or held down
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                fieldSelected(hit.point);
                return;
            }
        }
    }

    private void fieldSelected(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / gameView.CellWidth + gameView.CellWidth / 2);
        int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / gameView.CellHeight + gameView.CellHeight / 2);

        Debug.Log("zoneid at " + x + " " + z + " = " + gameData.getZoneId(x,z).ToString());
    }
}
