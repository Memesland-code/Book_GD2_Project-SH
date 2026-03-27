using System;

public class KnifeInstance
{
	public ItemData data;
	public float durability = 4;
	public Action onBroken;

	public bool isUsable => durability > 0;
	
	public void Use()
	{
		durability--;
		if (!isUsable) onBroken?.Invoke(); // Call inventory to be removed when broken
	}
}
