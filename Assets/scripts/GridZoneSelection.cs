using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Model;
using Assets.Model.Data;

public class GridZoneSelection : MonoBehaviour
{
    public SelectionBox selectionBox;

    private Camera mainCamera;
    private PlayerAction playerAction;
    private GameView gameView;
    private GridClickHandler gridClickHandler;

    void Start()
    {
        playerAction = GameModel.instance.playerAction;
        gameView = GetComponent<GameView>();
        gridClickHandler = GetComponent<GridClickHandler>();
        mainCamera = Camera.main;

        playerAction.OnZoneSelected += HandleZoneSelected;

        UnityThread.initUnityThread();
    }

    private void OnApplicationQuit()
    {
        playerAction.OnZoneSelected -= HandleZoneSelected;
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
        if (gridClickHandler.ZoneButtonSelected) return;

        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / gameView.CellWidth + gameView.CellWidth / 2);
        int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / gameView.CellHeight + gameView.CellHeight / 2);

        //Debug.Log("zoneid at " + x + " " + z + " = " + gameData.getZoneId(x,z).ToString());
        selectionBox.SetVisible(false);
        playerAction.SelectZone(x, z);
    }

    private void HandleZoneSelected(int tx, int tz, int bx, int bz)
    {
        float w = gameView.CellWidth / 2;
        float h = gameView.cellHeight / 2;

        selectionBox.UpdateSelectionBox(new Vector3(tx - w, 0, tz - h), new Vector3(bx + w, 0, bz + h));
        selectionBox.SetVisible(true);
    }
}
