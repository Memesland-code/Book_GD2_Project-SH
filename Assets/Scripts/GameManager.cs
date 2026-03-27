using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[HideInInspector] public GameObject player;
	[HideInInspector] public PlayerController playerController;
	[HideInInspector] public GameObject uiCanvas;

	[HideInInspector] public WeaponUI weaponUi;

	private float reviveTime;
	private bool shouldRevive;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void Start()
    {
	    ReloadReferences();
    }

	private void Update()
	{
		if (Time.time >= reviveTime && shouldRevive)
		{
			shouldRevive = false;
			player.SetActive(true);
			player.GetComponent<PlayerController>().Revive();
		}
	}


	private void ReloadReferences()
	{
		if (player == null)
			player = GameObject.FindGameObjectWithTag("Player");
		
		if (player != null)
			playerController = player.GetComponent<PlayerController>();
	    
		if (uiCanvas == null)
			uiCanvas = GameObject.FindGameObjectWithTag("MainUI");

		if (uiCanvas != null)
			weaponUi = uiCanvas.GetComponent<WeaponUI>();
		else
			Debug.LogError("UiCanvas not found");
	}


	public WeaponUI GetWeaponUi()
	{
		if (weaponUi == null)
			ReloadReferences();

		return weaponUi;
	}

	
	
	public void ManagePlayerDeath()
	{
		reviveTime = Time.time + 3;
		shouldRevive = true;
	}

	
	
	public (float, int) PollPlayerHealthAndAmmo()
	{
		return playerController.GetPotentialHealthAndAmmo();
	}
	
	
	
	public ItemInstance SelectLoot(List<LootTier> lootTiers)
	{
		// Get current player health (+ counts unused health packs potential) and ammo stats
		(float playerPotentialHealth, int playerAmmo) = Instance.PollPlayerHealthAndAmmo();

		// Select the loot pool tier depending on player's stats
		foreach (var tier in lootTiers)
		{
			if (playerPotentialHealth < tier.healthThreshold || playerAmmo < tier.ammoThreshold)
				return ResolveEntry(GetRandomItemFromTier(tier));
		}

		// If no tier triggered by threshold (= player is fine) select last tier where every loot is random (same weight)
		return ResolveEntry(GetRandomItemFromTier(lootTiers.Last()));
	}



	private LootEntry GetRandomItemFromTier(LootTier tier)
	{
		print("Tier selected for drop: " + tier.name);
		
		// Get the sum of all the items weights
		float total = tier.possibleLoots.Sum(e => e.weight);
		
		// Roll a random number between 0 and weight sum
		float roll = Random.Range(0, total);
		float current = 0f;

		// Select the item depending on the rolled number (basic random selection function)
		foreach (var entry in tier.possibleLoots)
		{
			current += entry.weight;
			if (roll <= current) return entry;
		}

		Debug.LogError("GameManager:GetRandomItemFromTier: Unreachable code reached!\nCheck for correct setup of LootTiers and LootEntries.");
		return tier.possibleLoots.Last(); // Fallback if setup problem
	}



	private ItemInstance ResolveEntry(LootEntry entry)
	{
		return new ItemInstance
		{
			data = entry.itemData,
			resolvedValue = entry.valueRange != Vector2.zero
			? Random.Range(entry.valueRange.x, entry.valueRange.y)
			: 1
		};
	}
}
