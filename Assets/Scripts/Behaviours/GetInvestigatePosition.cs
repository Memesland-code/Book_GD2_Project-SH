using System;
using UnityEngine;

public class GetInvestigatePosition : MonoBehaviour
{
	private Vector3 position;
	
	public Vector3 GetTargetPosition(GameObject target)
	{
		return target.transform.position;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.firebrick;
		Gizmos.DrawSphere(position, 0.1f);
	}
}
