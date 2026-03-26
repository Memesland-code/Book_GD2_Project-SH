using UnityEngine;

namespace Behaviours
{
	public class LineOfSightDetector : MonoBehaviour
	{
		[Header("Detection Settings")]
		[SerializeField] private float detectionAngle = 45f;
		
		[Header("Debug and offset parameters")]
		[SerializeField] private float thisYOffset = 1.4f;
		[SerializeField] private bool showDebugVisuals;
		
		private bool isInAngle;
		private bool isVisible;
		
		private Vector3 side1;
		private Vector3 side2;
		
		public GameObject DetectedTarget
		{
			get;
			set;
		}
		
		public GameObject FilterDetectedTarget(GameObject target, bool checkSight, bool checkAngle)
		{
			DetectedTarget = null;
			isInAngle = false;
			isVisible = false;

			RaycastHit hit;
			
			if (checkSight)
			{
				Debug.DrawRay(transform.position + new Vector3(0, thisYOffset, 0), (target.transform.position - transform.position), Color.aquamarine, 0.1f);
				if (Physics.Raycast(transform.position + new Vector3(0, thisYOffset, 0), (target.transform.position - transform.position), out hit, Mathf.Infinity))
				{
					if (hit.transform.CompareTag(target.tag))
					{
						isVisible = true;
					}
				}
			}

			
			side1 = target.transform.position - transform.position;
			side2 = transform.forward;
			float angle = Vector3.SignedAngle(side1, side2, Vector3.up);
			
			if (checkAngle)
			{
				if (angle < detectionAngle && angle > -1 * detectionAngle)
				{
					isInAngle = true;
				}
			}

			if (checkSight && checkAngle)
			{
				if (isVisible && isInAngle) DetectedTarget = target;
				else DetectedTarget = null;
			}
			else if (checkSight)
			{
				DetectedTarget = isVisible ? target : null;
			}
			else if (checkAngle)
			{
				DetectedTarget = isInAngle ? target : null;
			}
			else
			{
				Debug.LogError("Source Error: " + this.name + " - " + this.gameObject + "\nNo checking condition ticked in LineOfSightDetector!\nThis may cause unwanted behaviours!");
			}
			
			return DetectedTarget;
		}
		
		
		private void OnDrawGizmos()
		{
			if (!showDebugVisuals || this.enabled == false) return;

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
