using Player;
using UnityEngine;

public class PlayerDamager : MonoBehaviour
{
	[SerializeField] private float zoneDamage;
	[SerializeField] private bool isOneTime;
	private bool alreadyHit;
	
	private void OnTriggerEnter(Collider collider)
	{
		if (collider.TryGetComponent(out PlayerController playerController))
		{
			if (isOneTime && alreadyHit) return;
		
			if (isOneTime) alreadyHit = true;

			playerController.TakeDamage(zoneDamage, gameObject);
		}
	}
}
