using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeUI : MonoBehaviour
{
	[SerializeField] private Image playerLifeImage;
	
	[SerializeField] Color excellentLifeColor;
	[SerializeField] Color crticialLifeColor;

	public void SetPlayerLifePercent(float percent)
	{
		playerLifeImage.fillAmount = percent;

		float hue = Mathf.Lerp(0, 0.33f, percent);
		playerLifeImage.color = Color.HSVToRGB(hue, 1, 1);
	}
}
