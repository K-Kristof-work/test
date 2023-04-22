using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sidebar : MonoBehaviour
{
    public TextMeshProUGUI title;
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
        anim.Play("Slide");
    }

    public void Close()
    {
        anim.Play("SlideBack");
    }

    private void SetTitle(SidebarPanel panel)
    {
        switch (panel)
        {
            case SidebarPanel.Road:
                title.SetText("Utak");
                break;
            case SidebarPanel.ResidentialZone:
                title.SetText("Lak� z�na");
                break;
            case SidebarPanel.IndustrialZone:
                title.SetText("Ipari z�na");
                break;
            case SidebarPanel.CommercialZone:
                title.SetText("Szolg�ltat�s z�na");
                break;
        }
    }
}

public enum SidebarPanel { Road, ResidentialZone, IndustrialZone, CommercialZone };
