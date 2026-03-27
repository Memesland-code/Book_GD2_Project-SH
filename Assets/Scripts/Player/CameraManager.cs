using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
	public class CameraManager : MonoBehaviour
	{
		private InputManager inputs;

		[Header("Basic controls")]
		[SerializeField] private float mouseSensivity;
		[SerializeField] private Transform playerBody;
		[SerializeField] private Transform cameraPos;
		[SerializeField] private float topClamp = 75f;
		[SerializeField] private float bottomClamp = -90f;
		[SerializeField] private float standingNearClippingFrame;
		[SerializeField] private float crouchingNearClippingFrame;
		
		
		[Space(10)]
		[Header("Gameplay")]
		[SerializeField] private float baseFOV;
		[SerializeField] private float aimingFOV;
		[SerializeField] private float aimTransitionSpeed;
		
		[HideInInspector] public CinemachineCamera cam;
		private float xRotation;

		private void Awake()
		{
			inputs = GetComponent<InputManager>();
		}

		private void Start()
		{ 
			cam = GameObject.FindGameObjectWithTag("CinemachineCam").GetComponent<CinemachineCamera>();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void Update()
		{
			float mouseX = inputs.lookInput.x * mouseSensivity * Time.deltaTime;
			float mouseY = inputs.lookInput.y * mouseSensivity * Time.deltaTime;
			
			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);
			
			cameraPos.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
			
			playerBody.Rotate(Vector3.up * mouseX);
		}

		public void SetCameraPosition(bool isCrouched, Vector3 targetCameraPosition, float crouchTransitionSpeed)
		{
			cam.Lens.NearClipPlane = isCrouched ? crouchingNearClippingFrame : standingNearClippingFrame;

			cameraPos.transform.localPosition = Vector3.Lerp(cameraPos.localPosition, targetCameraPosition, crouchTransitionSpeed * Time.deltaTime);
		}

		public void SetCameraFOV(bool aim, bool dead)
		{
			if (dead)
				cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, 120, aimTransitionSpeed * Time.deltaTime);
			else
				cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, aim ? aimingFOV : baseFOV, aimTransitionSpeed * Time.deltaTime);
		}
	}
}
