using System;
using Player;
using UnityEngine;

[ExecuteInEditMode]
public class Item : MonoBehaviour, IPickable
{
	[SerializeField] private ItemData data;
	[SerializeField] private float resolvedValue;
	
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

	private void Start()
	{
		if (resolvedValue == 0) Debug.LogError("Error on item: " + data.name + "\n" +
		                                       "At position: " + transform.position + "\n" +
		                                       "Item resolved value is " + resolvedValue);
	}

	public void OnPickUp(PlayerController playerController)
	{
		switch (data.type)
		{
			case ItemType.HealPack:
				playerController.AddHealthPack(data, Mathf.RoundToInt(resolvedValue));
				break;
			
			case ItemType.AmmoBox:
				playerController.playerWeapon.PickupAmmo(Mathf.RoundToInt(resolvedValue));
				break;
			
			case ItemType.MakeshiftKnife:
				playerController.AddKnife(data, Mathf.RoundToInt(resolvedValue));
				break;
			
			default:
				throw new ArgumentOutOfRangeException();
		}
		Destroy(gameObject);
	}
}