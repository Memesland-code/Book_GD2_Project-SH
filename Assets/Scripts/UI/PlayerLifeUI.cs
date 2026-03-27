using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeUI : MonoBehaviour
{
	[SerializeField] private Image playerLifeImage;

	public void SetPlayerLifePercent(float percent)
	{
		playerLifeImage.fillAmount = percent;
	}
}
