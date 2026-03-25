using UnityEngine;

namespace Behaviours
{
	public class RangeDetector : MonoBehaviour
	{
		[Header("Detection Settings")]
		[SerializeField] private float detectionRadius = 10f;
		[SerializeField] private LayerMask detectionMask;
		[SerializeField] private bool showDebugVisuals = true;

		private bool isInAngle;
		private bool isInRange;
		private bool isNotHidden;

		private GameManager player;

		public GameObject DetectedTarget
		{
			get;
			set;
		}

		public GameObject UpdateDetector()
		{
			isInAngle = false;
			isInRange = false;
			isNotHidden = false;

			if (Vector3.Distance(transform.position, player.transform.position) < detectionRadius)
			{
				isInRange = true;
			}
			
			
			
			Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionMask);

			if (colliders.Length > 0)
			{
				DetectedTarget = colliders[0].gameObject;
			}
			else
			{
				DetectedTarget = null;
			}
			return DetectedTarget;
		}

		private void OnDrawGizmos()
		{
			if (!showDebugVisuals || this.enabled == false) return;

			Gizmos.color = DetectedTarget ? Color.green : Color.yellow;
			Gizmos.DrawWireSphere(transform.position, detectionRadius);
		}
	}
}