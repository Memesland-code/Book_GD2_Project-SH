using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
	public AudioMixer audioMixer;

	private Resolution[] resolutions;
	public TMP_Dropdown resolutionDropdown;
	
	private void Start()
	{
		resolutions = Screen.resolutions;
		
		resolutionDropdown.ClearOptions();
		
		List<string> options = new List<string>();

		int currentResolutionIndex = 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			string option = resolutions[i].width + " x " + resolutions[i].height;
			options.Add(option);

			if (resolutions[i].width == Screen.currentResolution.width &&
			    resolutions[i].height == Screen.currentResolution.height)
			{
				currentResolutionIndex = i;
			}
		}
		
		resolutionDropdown.AddOptions(options);
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();
	}
	
	public void CloseMenu()
	{
		gameObject.SetActive(false);
	}

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
	}
	
	public void SetFullscreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}

	public void SetResolution(int resolutionIndex)
	{
		Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, Screen.fullScreen);
	}

	public void SetVolume(float volume)
	{
		audioMixer.SetFloat("MasterVolume", volume);
	}

	public void SetMouseSensivity(float sensivity)
	{
		GameManager.Instance.SetMouseSensivity(sensivity);
	}
}
