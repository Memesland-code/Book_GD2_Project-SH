using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData itemData;
    public float weight;

    [Header("Min inclusive, Max inclusive")]
    public Vector2 valueRange;
}

[System.Serializable]
public class LootTier
{
	public string name;
	[Range(0, 100)] public float healthThreshold;
	[Range(0, 100)] public float ammoThreshold;
	public List<LootEntry> possibleLoots;
}

[CreateAssetMenu(menuName = "Items/LootTable")]
public class LootTable : ScriptableObject
{
	public List<LootTier> tiers;
}
