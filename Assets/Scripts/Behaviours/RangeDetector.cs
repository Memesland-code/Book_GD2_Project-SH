using System;
using UnityEngine;

namespace Behaviours
{
	public class RangeDetector : MonoBehaviour
	{
		[Header("Detection Settings")]
		[SerializeField] private LayerMask detectionMask;

		[Header("Debug")]
		[SerializeField] private bool showDebugVisuals = true;

		private bool isInRange;
		private float detectionValue;
		
		public GameObject DetectedTarget
		{
			get;
			set;
		}
		
		public GameObject UpdateDetector(float detectionRadius)
		{
			detectionValue = detectionRadius;
			
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

		private void OnDrawGizmos()
		{
			if (!showDebugVisuals) return;
			
			Gizmos.color = isInRange ? Color.green : Color.red;
			Gizmos.DrawWireSphere(transform.position, detectionValue);
		}
	}
}
