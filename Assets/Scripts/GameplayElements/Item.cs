using System;
using Player;
using UnityEngine;

[ExecuteInEditMode]
public class Item : MonoBehaviour, IPickable
{
	[SerializeField] private ItemData data;
	private float resolvedValue;
	
	[SerializeField] private Vector3 rotationOffset;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private MeshCollider meshCollider;

	public void Initalize(ItemInstance instance)
	{
		data = instance.data;
		resolvedValue = instance.resolvedValue;
		OnValidate();
	}

	private void OnValidate()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		
		if (data != null)
		{
			meshFilter.mesh = data.mesh;
			meshRenderer.materials = data.materials;
			meshCollider.sharedMesh = data.mesh;
		}
		
		gameObject.transform.rotation = Quaternion.Euler(rotationOffset);
	}

	public void OnPickUp(PlayerController playerController)
	{
		switch (data.type)
		{
			case ItemType.HealPack:
				//TODO add heal pack
				break;
			
			case ItemType.AmmoBox:
				//TODO add ammo box
				break;
			
			case ItemType.MakeshiftKnife:
				playerController.AddKnife(data, 100);
				break;
			
			default:
				throw new ArgumentOutOfRangeException();
		}
		Destroy(gameObject);
	}
}