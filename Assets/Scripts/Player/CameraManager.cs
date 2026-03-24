using UnityEngine;

namespace Player
{
	public class CameraManager : MonoBehaviour
	{
		private InputManager inputs;

		[SerializeField] private float mouseSensivity;
		private Camera cam;

		private float xRotation;

		[SerializeField] private float topClamp = 90f;
		[SerializeField] private float bottomClamp = -90f;

		private void Awake()
		{
			inputs = GetComponent<InputManager>();
		}

		private void Start()
		{
			cam = FindFirstObjectByType<Camera>();
			
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void Update()
		{
			float mouseX = inputs.lookInput.x * mouseSensivity * Time.deltaTime;
			float mouseY = inputs.lookInput.y * mouseSensivity * Time.deltaTime;
			
			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);
			
			cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
			
			transform.Rotate(Vector3.up * mouseX);
		}

		public void SetAiming(bool aim)
		{
			if (aim)
			{
				cam.fieldOfView = 70;
			}
			else
			{
				cam.fieldOfView = 90;
			}
		}
	}
}
