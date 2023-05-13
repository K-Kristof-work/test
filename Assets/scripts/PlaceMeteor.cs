using Assets.Model.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceMeteor : MonoBehaviour
{
    private Camera mainCamera;
    private GameView gameView;

    public GameObject prefab_cylinder;
    public Button MeteoritButton;
    public Color NormalColor;
    public Color SelectedColor;
    public int size = 5;

    private GameObject cylinder;
    private bool btn_active = false;
    private List<Vector2> clear = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        gameView = GetComponent<GameView>();

        MeteoritButton.onClick.AddListener(() =>
        {
            btn_active = !btn_active;
            if (!btn_active && cylinder)
            {
                Destroy(cylinder);
            }

            UpdateButtonColor();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(btn_active)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                hoverEffect(hit.point);
            }
        }

    }

    private void hoverEffect(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / gameView.CellWidth + gameView.CellWidth / 2);
        int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / gameView.CellHeight + gameView.CellHeight / 2);

        if(cylinder == null)
        {
            cylinder = Instantiate(prefab_cylinder);
            cylinder.transform.localScale = new Vector3(size, 0.01f, size);
            cylinder.transform.parent = this.transform;
        }

        cylinder.transform.position = new Vector3(x, 0, z);
    }

    private void UpdateButtonColor()
    {
        ColorBlock buttonColors = MeteoritButton.colors;

        buttonColors.normalColor = btn_active ? SelectedColor : NormalColor;
        buttonColors.highlightedColor = btn_active ? SelectedColor : NormalColor;
        buttonColors.pressedColor = btn_active ? SelectedColor : NormalColor;
        buttonColors.selectedColor = btn_active ? SelectedColor : NormalColor;

        MeteoritButton.colors = buttonColors;
    }
}
