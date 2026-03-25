using UnityEngine;

namespace Behaviours
{
	public class RangeDetector : MonoBehaviour
	{
		[Header("Detection Settings")]
		[SerializeField] private float detectionRadius = 10f;
		[SerializeField] private float overrideDetectionRadius = 1;
		[SerializeField] private LayerMask detectionMask;
		[SerializeField] private float detectionAngle = 45f;
		
		[Header("Debug and offset parameters")]
		[SerializeField] private float thisYOffset = 1.4f;
		[SerializeField] private bool showDebugVisuals = true;

		private bool isInRange;
		private bool isInAngle;
		private bool isVisible;
		private bool isInOverrideDetection;

		private Vector3 side1;
		private Vector3 side2;

		public GameObject DetectedTarget
		{
			get;
			set;
		}
		
		private GameObject affinedDetectedTarget;

		public GameObject UpdateDetector()
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionMask);

			if (colliders.Length > 0)
			{
				DetectedTarget = colliders[0].gameObject;
				isInRange = true;
			}
			else
			{
				DetectedTarget = null;
				isInRange = false;
			}
			return DetectedTarget;
		}

		public GameObject FilterDetectedTarget(GameObject target)
		{
			affinedDetectedTarget = null;
			isInAngle = false;
			isVisible = false;
			isInOverrideDetection = false;
			
			// If player is too close from enemy, it will be spotted even from behind
			Collider[] colliders = Physics.OverlapSphere(transform.position, overrideDetectionRadius, detectionMask);
			if (colliders.Length > 0)
			{
				isInOverrideDetection = true;
				return target;
			}
			
			RaycastHit hit;
			
			Debug.DrawRay(transform.position + new Vector3(0, thisYOffset, 0), (target.transform.position - transform.position), Color.aquamarine, 0.1f);
			if (Physics.Raycast(transform.position + new Vector3(0, thisYOffset, 0), (target.transform.position - transform.position), out hit, Mathf.Infinity))
			{
				if (hit.transform.CompareTag(target.tag))
				{
					isVisible = true;
				}
			}
			
			side1 = target.transform.position - transform.position;
			side2 = transform.forward;
			float angle = Vector3.SignedAngle(side1, side2, Vector3.up);
			
			if (angle < detectionAngle && angle > -1 * detectionAngle)
			{
				isInAngle = true;
			}
			
			if (isVisible && isInAngle)
				affinedDetectedTarget = target;
			
			return affinedDetectedTarget;
		}

		private void OnDrawGizmos()
		{
			if (!showDebugVisuals || this.enabled == false) return;
			
			Gizmos.color = isInRange ? Color.green : Color.red;
			Gizmos.DrawWireSphere(transform.position, detectionRadius);
			
			Gizmos.color = isInOverrideDetection ? Color.darkGreen : Color.darkRed;
			Gizmos.DrawWireSphere(transform.position, overrideDetectionRadius);

			if (DetectedTarget != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(transform.position + new Vector3(0, thisYOffset, 0), transform.forward * 250);
			
				Gizmos.color = isInAngle ? Color.green : Color.red;
				Gizmos.DrawLine(transform.position + new Vector3(0, thisYOffset, 0), DetectedTarget.transform.position + new Vector3(0, 1.4f, 0));
			}
		}
	}
}