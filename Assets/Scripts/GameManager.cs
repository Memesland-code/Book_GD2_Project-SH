using UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[HideInInspector] public GameObject player;
	[HideInInspector] public GameObject uiCanvas;

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
		if (player == null)
			player = GameObject.FindGameObjectWithTag("Player");
	    
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
}
