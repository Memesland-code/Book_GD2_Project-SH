using System;

public class KnifeInstance
{
	public ItemData Data;
	public float Durability = 4;
	public Action OnBroken;

	public bool IsUsable => Durability > 0;
	
	public void Use()
	{
		Durability--;
		if (!IsUsable) OnBroken?.Invoke(); // Call inventory to be removed when broken
	}
}
