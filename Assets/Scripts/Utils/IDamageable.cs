using UnityEngine;

public interface IDamageable
{
	void TakeDamage(float damageAmount, GameObject damageSource);
	
	void Heal(float healAmount);

	void Revive();

	void RunTriggerDetection(Collider otherCollider);
}
