using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnifeUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI knivesNumberText;
	[SerializeField] private Image knifeLifeSlider;

	[SerializeField] private Color highLifeColor;
	[SerializeField] private Color lowLifeColor;
	[SerializeField] private float lowLifeThreshold;

	public void RefreshKnivesUI(int knivesNumber, float currentKnifeLife)
	{
		knivesNumberText.text = knivesNumber.ToString();
		knifeLifeSlider.fillAmount = currentKnifeLife / 5f;

		knifeLifeSlider.color = currentKnifeLife / 5f <= 0.2f ? lowLifeColor : highLifeColor;
	}
}
