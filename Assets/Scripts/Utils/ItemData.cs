using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
	public GameObject prefab;
	
	public int internalID;
	public ItemType type;
	public string itemName;
	public Mesh mesh;
	public Material[] materials;

	public float value; // Generic value contained in item
}

public class ItemInstance
{
	public ItemData data;
	public float resolvedValue;
}