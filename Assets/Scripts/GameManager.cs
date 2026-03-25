using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[HideInInspector] public GameObject Player;
	[HideInInspector] public GameObject UiCanvas;

	[HideInInspector] public WeaponUI weaponUi;

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


	private void ReloadReferences()
	{
		if (Player == null)
			Player = GameObject.FindGameObjectWithTag("Player");
	    
		if (UiCanvas == null)
			UiCanvas = GameObject.FindGameObjectWithTag("MainUI");

		if (UiCanvas != null)
			weaponUi = UiCanvas.GetComponent<WeaponUI>();
		else
			Debug.LogError("UiCanvas not found");
	}


	public WeaponUI GetWeaponUi()
	{
		if (weaponUi == null)
			ReloadReferences();

		return weaponUi;
	}
}
