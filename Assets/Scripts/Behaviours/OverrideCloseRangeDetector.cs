using System;
using Player;
using UnityEngine;

namespace Behaviours
{
	public class OverrideCloseRangeDetector : MonoBehaviour
	{
		[Header("Detection Settings")]
		[SerializeField] private float overrideDetectionRadius;
		[SerializeField] private LayerMask detectionMask;
		
		[Header("Debug Settings")]
		[SerializeField] private bool showDebugVisuals;

		private bool isDetected;
		
		public GameObject UpdateOverrideRangeDetector()
		{
			isDetected = false;
			
			// If player is too close from enemy, it will be spotted even from behind
			Collider[] colliders = Physics.OverlapSphere(transform.position, overrideDetectionRadius, detectionMask);
			if (colliders.Length > 0)
			{
				if (colliders[0].CompareTag("Player") && colliders[0].TryGetComponent(out InputManager inputManager))
				{
					if (inputManager.isCrouched) return null;
					isDetected = true;
				}
				return colliders[0].gameObject;
			}
			
			return null;
		}

		private void OnDrawGizmos()
		{
			if (!showDebugVisuals) return;
			
			Gizmos.color = isDetected ? Color.darkGreen : Color.darkRed;
			Gizmos.DrawWireSphere(transform.position, overrideDetectionRadius);
		}
	}
}
