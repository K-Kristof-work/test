using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SidebarContent : MonoBehaviour
{
    public TextMeshProUGUI fullness;
    public TextMeshProUGUI capacity;
    public TextMeshProUGUI taxes;
    public TextMeshProUGUI level;
    public TextMeshProUGUI happiness;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
    }

    public void Hide()
    {
        base.gameObject.SetActive(false);
    }
}
