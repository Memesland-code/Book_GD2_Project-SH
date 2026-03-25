using TMPro;
using UnityEngine;

namespace UI
{
	public class WeaponUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI ammoText;

		public void SetAmmoText(int magazineAmmo, int inventoryAmmo)
		{
			ammoText.text = magazineAmmo + "/" + inventoryAmmo;
		}
	}
}
