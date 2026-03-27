using Player;

public interface IPickable
{
	void OnPickUp(PlayerController playerController);
}

public enum ItemType
{
	HealPack,
	AmmoBox,
	MakeshiftKnife
}