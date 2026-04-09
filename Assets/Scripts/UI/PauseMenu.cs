using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject optionsMenu;

	public void OnEnable()
	{
		Time.timeScale = 0;
	}

	public void OnResume()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Time.timeScale = 1;
		gameObject.SetActive(false);
	}

	public void OnOptionsMenuClick()
	{
		optionsMenu.SetActive(true);
	}

	public void OnMainMenuClick()
	{
		SceneManager.LoadScene("MainMenu");
	}
}
