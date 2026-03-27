using UnityEngine;

public class ItemCrate : MonoBehaviour, IDamageable, ILootable
{
	[SerializeField] private LootTable lootTable;
	[SerializeField] private float breakHearRadius;
	
	public void TakeDamage(float damageAmount, GameObject damageSource)
	{
		//TODO SFX break
		
		LootItem(GameManager.Instance.SelectLoot(lootTable.tiers));
		
		SoundSystem.EmitSound(transform.position, breakHearRadius, gameObject);
		gameObject.SetActive(false);
	}
	
	
	
	public void Heal(float healAmount)
	{
		throw new System.NotImplementedException();
	}

	public void Revive()
	{
		throw new System.NotImplementedException();
	}

	public void OnAttackCollision(Collider otherCollider)
	{
		throw new System.NotImplementedException();
	}

	public void LootItem(ItemInstance lootItem)
	{
		Vector3 spawnPosition;
		
		// Check ground position if close
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
			spawnPosition = hit.point;
		else
			spawnPosition = transform.position;
		
		// Spawn item at ground (or box position if in air)
		GameObject spawnedItem = Instantiate(lootItem.data.prefab, transform.position, Quaternion.identity);
		
		spawnedItem.GetComponent<Item>().Initalize(lootItem);
		
		// Reset position considering halfHeight to avoid in-ground spawn
		float itemHalfHeight = spawnedItem.GetComponent<Collider>().bounds.extents.y;
		spawnedItem.transform.position = spawnPosition + Vector3.up * itemHalfHeight;
	}


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.orange;
		Gizmos.DrawWireSphere(transform.position, breakHearRadius);
	}
}
