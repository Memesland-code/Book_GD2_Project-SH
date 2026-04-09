using UnityEngine;

public interface IDamageable
{
	void TakeDamage(float damageAmount, GameObject damageSource, bool ignoreCooldown=false);
	
	void Heal(float healAmount);

	void Revive();

	void OnAttackCollision(Collider otherCollider, bool isRadialAttack);
}
