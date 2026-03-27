using UnityEngine;

public interface IPickable
{
	void OnPickUp();
}

public enum ItemType
{
	HealPack,
	AmmoBox,
	MakeshiftKnife
}