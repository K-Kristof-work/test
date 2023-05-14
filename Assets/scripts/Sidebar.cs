using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sidebar : MonoBehaviour
{
    public TextMeshProUGUI title;
    public List<string> titleList;
    public List<SidebarContent> contentList;
    private Animator anim;
    private bool visible = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = base.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(SidebarPanel panel)
    {
        SetTitle(panel);
        DisplayContent(panel);
        anim.Play("Slide");
        visible = true;
    }

    public void Close()
    {
        if (!visible) return;

        visible = false;
        anim.Play("SlideBack");
    }

    private void SetTitle(SidebarPanel panel)
    {
        title.SetText(titleList[(int)panel]);
    }

    public void SetDetails(SidebarPanel panel, int fullness, int capacity, int taxes, int level, int happiness)
    {
        SidebarContent sc = contentList[(int)panel];
        sc.fullness.SetText(fullness.ToString() + '%');
        sc.capacity.SetText(capacity.ToString());
        sc.taxes.SetText(taxes.ToString() + '%');
        sc.level.SetText(level.ToString());
        sc.happiness.SetText(happiness.ToString() + '%');
    }

    private void DisplayContent(SidebarPanel panel)
    {
        foreach (var item in contentList)
        {
            item.Hide();
        }

        contentList[(int)panel].Show();
    }
}

public enum SidebarPanel { Road, BuildResidentialZone, BuildIndustrialZone, BuildCommercialZone, ResidentialZone, IndustrialZone, CommercialZone };
