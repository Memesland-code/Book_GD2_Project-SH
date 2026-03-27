using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KnifeUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI knivesNumberText;
	[SerializeField] private Image knifeLifeSlider;

	public void RefreshKnivesUI(int knivesNumber, float currentKnifeLife)
	{
		knivesNumberText.text = knivesNumber.ToString();
		knifeLifeSlider.fillAmount = currentKnifeLife / 5;
	}
}
