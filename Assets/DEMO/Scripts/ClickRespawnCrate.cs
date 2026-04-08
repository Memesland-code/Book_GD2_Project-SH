using UnityEngine;

public class ClickRespawnCrate : MonoBehaviour, IInteractable
{
	[SerializeField] private GameObject crate;
	
	public void Interact(GameObject interactor)
	{
		crate.SetActive(true);
	}
}
