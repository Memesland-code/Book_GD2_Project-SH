using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPacksUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthPacksNumberText;

    [SerializeField] private Image currentHealthPackFillAmount;
    [SerializeField] TextMeshProUGUI currentHealthPackHealAmountText;

    public void SetHealthPacksInfo(int packsNumber, int currentPackHealAmount)
    {
	    healthPacksNumberText.text = packsNumber.ToString();
	    
	    currentHealthPackFillAmount.fillAmount = Mathf.Lerp(0.156f, 0.47f, (float)(currentPackHealAmount / 100.0));
	    currentHealthPackHealAmountText.text = currentPackHealAmount.ToString();
    }
}
