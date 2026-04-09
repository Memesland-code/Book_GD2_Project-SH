using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/LootTable")]
public class LootTable : ScriptableObject
{
	public List<LootTier> tiers;
}