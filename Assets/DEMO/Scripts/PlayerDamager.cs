using Player;
using UnityEngine;

public class PlayerDamager : MonoBehaviour
{
	[SerializeField] private float zoneDamage;
	[SerializeField] private bool isOneTime;
	[SerializeField] private bool isSetHealth;
	private bool alreadyHit;
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out PlayerController playerController))
		{
			if (isOneTime && alreadyHit) return;
			if (isOneTime) alreadyHit = true;
			
			if (!isSetHealth) playerController.TakeDamage(zoneDamage, gameObject, false);

			if (isSetHealth)
			{
				playerController.SetCurrentHealth(zoneDamage);
				playerController.UpdateHealthUI();
			}
		}
	}
}
