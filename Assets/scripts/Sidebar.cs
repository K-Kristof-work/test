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
    }

    public void Close()
    {
        anim.Play("SlideBack");
    }

    private void SetTitle(SidebarPanel panel)
    {
        title.SetText(titleList[(int)panel]);
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

public enum SidebarPanel { Road, ResidentialZone, IndustrialZone, CommercialZone };
