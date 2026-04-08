using UnityEngine;

public class CheckPoint : MonoBehaviour
{
	public Vector3 GetCheckpointPosition()
	{
		return transform.GetChild(0).position;
	}
}
