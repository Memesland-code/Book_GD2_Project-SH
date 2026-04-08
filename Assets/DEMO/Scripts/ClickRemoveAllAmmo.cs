using Player;
using UnityEngine;

public class ClickRemoveAllAmmo : MonoBehaviour, IInteractable
{
	public void Interact(GameObject interactor)
	{
		if (interactor.TryGetComponent(out PlayerWeapon player))
		{
			player.RemoveAllAmmo();
		}
	}
}
