using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData itemData;
    public float weight;

    [Header("Possible value range of the item (Min inclusive, Max inclusive)")]
    public Vector2 valueRange;
}

[System.Serializable]
public class LootTier
{
	public string name;
	[Tooltip("Will trigger if actual number is strictly inferior"), Range(0, 100)] public float healthThreshold;
	[Tooltip("Will trigger if actual number is strictly inferior"), Range(0, 100)] public float ammoThreshold;
	public List<LootEntry> possibleLoots;
}

[CreateAssetMenu(menuName = "Items/LootTable")]
public class LootTable : ScriptableObject
{
	public List<LootTier> tiers;
}
