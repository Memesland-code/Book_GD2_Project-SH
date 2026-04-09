using Player;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
	[SerializeField] private bool resetHealthPacks;
	[SerializeField] private bool resetammo;
	[SerializeField] private bool resetknives;
	
	public Vector3 GetCheckpointPosition()
	{
		if (resetHealthPacks)
		{
			GameManager.Instance.playerController.HealthPacks.Clear();
			GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(0, 0);
		}

		if (resetammo)
		{
			GameManager.Instance.player.GetComponent<PlayerWeapon>().RemoveAllAmmo();
		}

		if (resetknives)
		{
			GameManager.Instance.playerController.Knives.Clear();
			GameManager.Instance.playerController.RefreshKnifeVisuals();
		}
		
		return transform.GetChild(0).position;
	}
}
