using UnityEngine;

public class HitDetectZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		bool attackAllies = false;
			
		if (GetComponentInParent<SoundSensitiveZombie>() != null)
		{
			attackAllies = GetComponentInParent<SoundSensitiveZombie>().bbIsSoundSensitive.Value;
		}
		gameObject.GetComponentInParent<IDamageable>().OnAttackCollision(other, attackAllies);
	}
}
