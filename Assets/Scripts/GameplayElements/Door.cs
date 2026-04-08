using UnityEngine;

public class Door : MonoBehaviour, IDamageable
{
	[SerializeField] private AudioClip breakSound;
	
	public void TakeDamage(float damageAmount, GameObject damageSource)
	{
		AudioSource.PlayClipAtPoint(breakSound, transform.position);
		gameObject.SetActive(false);
	}
	
	
	
	public void Heal(float healAmount)
	{
		throw new System.NotImplementedException();
	}

	public void Revive()
	{
		throw new System.NotImplementedException();
	}

	public void OnAttackCollision(Collider otherCollider, bool isRadialAttack)
	{
		throw new System.NotImplementedException();
	}
}
