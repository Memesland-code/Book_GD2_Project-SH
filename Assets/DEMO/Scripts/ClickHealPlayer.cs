using UnityEngine;

public class ClickHealPlayer : MonoBehaviour, IInteractable
{
	public void Interact(GameObject interactor)
	{
		if (interactor.TryGetComponent(out IDamageable player))
		{
			player.Heal(10);
		}
	}
}
