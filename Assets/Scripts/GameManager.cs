using System.Collections.Generic;
using System.Linq;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindFirstObjectByType<GameManager>();
			}

			return _instance;
		}
		private set => _instance = value;
	}

	private static GameManager _instance;

	[HideInInspector] public GameObject player;
	[HideInInspector] public PlayerController playerController;
	[HideInInspector] public GameObject uiCanvas;

	private WeaponUI weaponUI;
	private KnifeUI knifeUI;
	private PlayerLifeUI playerLifeUI;
	private HealthPacksUI healthPacksUI;

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

	
	//* ===== References and Update Logic =====
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
		{
			weaponUI = uiCanvas.GetComponent<WeaponUI>();
			knifeUI = uiCanvas.GetComponent<KnifeUI>();
			playerLifeUI = uiCanvas.GetComponent<PlayerLifeUI>();
			healthPacksUI = uiCanvas.GetComponent<HealthPacksUI>();
		}
		else
			Debug.LogError("UiCanvas not found");
	}


	
	//* ===== Out scope scripts management =====
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
	
	
	
	//* ===== UI Management =====
	public WeaponUI GetWeaponUi()
	{
		if (weaponUI == null)
			ReloadReferences();

		return weaponUI;
	}

	public KnifeUI GetKnifeUI()
	{
		if (knifeUI == null)
			ReloadReferences();

		return knifeUI;
	}

	public PlayerLifeUI GetPlayerLifeUI()
	{
		if (playerLifeUI == null)
			ReloadReferences();
		
		return playerLifeUI;
	}

	public HealthPacksUI GetHealthPacksUI()
	{
		if (healthPacksUI == null)
			ReloadReferences();
		
		return healthPacksUI;
	}



	public void SetMouseSensivity(float sensivity)
	{
		player.GetComponent<CameraManager>().mouseSensivity = sensivity;
	}
}
