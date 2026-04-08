using Player;
using UnityEngine;

public class ClickDamagePlayer : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
	    if (interactor.TryGetComponent(out IDamageable player))
	    {
		    player.TakeDamage(10, gameObject);
	    }
    }
}
