using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
	[SerializeField] private GameObject optionsMenu;
	
	public void OnClickStartDemo()
	{
		SceneManager.LoadScene("SampleScene");
	}

	public void OnClickOptions()
	{
		optionsMenu.SetActive(true);
	}
	
	public void OnClickQuit()
	{
		Application.Quit();
	}
}
