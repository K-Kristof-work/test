using Assets.Model.Data;
using UnityEngine;
using UnityEngine.UI;

public class ZoneTypeSelector : MonoBehaviour
{
	public GridClickHandler gridClickHandler;
	public Button residentialButton;
	public Button commercialButton;
	public Button industrialButton;
	public Button roadButton;
	public Button emptyButton;
	public Color normalColor = Color.white;
	public Color selectedColor = Color.green;
	public Sidebar sidebar;
	private ZoneType selectedZoneType;

	void Start()
	{
		residentialButton.onClick.AddListener(() => SetSelectedZoneType(ZoneType.Residential));
		commercialButton.onClick.AddListener(() => SetSelectedZoneType(ZoneType.Commercial));
		industrialButton.onClick.AddListener(() => SetSelectedZoneType(ZoneType.Industrial));
		roadButton.onClick.AddListener(() => SetSelectedZoneType(ZoneType.Road));
		emptyButton.onClick.AddListener(() => SetSelectedZoneType(ZoneType.Empty));
	}

	private void SetSelectedZoneType(ZoneType zoneType)
	{
		if(selectedZoneType == zoneType && gridClickHandler.ZoneButtonSelected == true)
		{
			DisableTypeSelector();
			sidebar.Close();
			return;
		}
		gridClickHandler.ZoneButtonSelected = true;
		gridClickHandler.SelectedZoneType = zoneType;
		selectedZoneType = zoneType;
		UpdateButtonColors();
		OpenSidebar(zoneType);
	}

	private void OpenSidebar(ZoneType zoneType)
    {
        switch (zoneType)
        {
			case ZoneType.Road:
				sidebar.Open(SidebarPanel.Road);
				break;
			case ZoneType.Residential:
				sidebar.Open(SidebarPanel.ResidentialZone);
				break;
			case ZoneType.Industrial:
				sidebar.Open(SidebarPanel.IndustrialZone);
				break;
			case ZoneType.Commercial:
				sidebar.Open(SidebarPanel.CommercialZone);
				break;
		}
    }

	public void DisableTypeSelector()
	{
		gridClickHandler.ZoneButtonSelected = false;
		sidebar.Close();
		UpdateButtonColors();
	}

	private void UpdateButtonColors()
	{
		UpdateButtonColor(residentialButton, ZoneType.Residential);
		UpdateButtonColor(commercialButton, ZoneType.Commercial);
		UpdateButtonColor(industrialButton, ZoneType.Industrial);
		UpdateButtonColor(roadButton, ZoneType.Road);
		UpdateButtonColor(emptyButton, ZoneType.Empty);
	}

	private void UpdateButtonColor(Button button, ZoneType zoneType)
	{
		ColorBlock buttonColors = button.colors;
		bool isSelected = gridClickHandler.SelectedZoneType == zoneType;

		if(!gridClickHandler.ZoneButtonSelected)
		{
			isSelected = false;
		}

		buttonColors.normalColor = isSelected ? selectedColor : normalColor;
		buttonColors.highlightedColor = isSelected ? selectedColor : normalColor;
		buttonColors.pressedColor = isSelected ? selectedColor : normalColor;
		buttonColors.selectedColor = isSelected ? selectedColor : normalColor;

		button.colors = buttonColors;
	}
}
