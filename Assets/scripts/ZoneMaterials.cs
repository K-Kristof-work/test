using System.Collections.Generic;
using UnityEngine;

public class ZoneMaterials : MonoBehaviour
{
	public List<Material> emptyMaterials;
	public List<Material> residentialMaterials;
	public List<Material> commercialMaterials;
	public List<Material> industrialMaterials;
	public List<Material> TransperentMaterials;

	public Dictionary<ZoneType, List<Material>> Materials { get; private set; }

	private void Awake()
	{
		Materials = new Dictionary<ZoneType, List<Material>>
		{
			{ ZoneType.Empty, emptyMaterials },
			{ ZoneType.Residential, residentialMaterials },
			{ ZoneType.Commercial, commercialMaterials },
			{ ZoneType.Industrial, industrialMaterials },
			{ ZoneType.Road, TransperentMaterials },
			{ ZoneType.IncomingRoad, TransperentMaterials },
			{ ZoneType.Water, TransperentMaterials }
		};
	}

	public Material GetRandomMaterial(ZoneType zoneType)
	{
		//make a detailed debug log
		List<Material> materials = Materials[zoneType];
		//make a detailed debug log
		
		if (materials.Count > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, materials.Count);
			return materials[randomIndex];
		}
		//make a detailed debug log
		Debug.Log("No materials found for zone type " + zoneType);
		return null;
	}
}

public enum ZoneType
{
	Empty,
	Residential,
	Commercial,
	Industrial,
	Road,
	IncomingRoad,
	Water
}
