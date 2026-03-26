using UnityEngine;

public class HitDetectZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		gameObject.GetComponentInParent<IDamageable>().RunTriggerDetection(other);
	}
}
