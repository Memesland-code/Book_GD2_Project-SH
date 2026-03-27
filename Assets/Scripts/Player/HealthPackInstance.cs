using System;
using Player;

public class HealthPackInstance
{
	public ItemData Data;
	public float HealAmount;
	public Action OnUsed;

	public void Use(PlayerController player)
	{
		player.Heal(HealAmount);
		OnUsed?.Invoke();
	}
}
