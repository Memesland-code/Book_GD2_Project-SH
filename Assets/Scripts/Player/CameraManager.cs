using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
	public class CameraManager : MonoBehaviour
	{
		private static readonly int BlurStrength = Shader.PropertyToID("_BlurStrength");
		private static readonly int DistortStrength = Shader.PropertyToID("_DistortStrength");
		private static readonly int VignetteHorizontalAspect = Shader.PropertyToID("_vignetteHorizontalAspect");
		private static readonly int VignetteVerticalAspect = Shader.PropertyToID("_vignetteVerticalAspect");
		private InputManager input;
		private PlayerController playerController;
		private CinemachineBasicMultiChannelPerlin camNoise;

		[Header("Basic controls")]
		public float mouseSensivity;
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

		[SerializeField] private float minAmplitude;
		[SerializeField] private float maxAmplitude;
		[SerializeField] private float minFrequency;
		[SerializeField] private float maxFrequency;
		
		[HideInInspector] public CinemachineCamera cam;
		private float xRotation;

		[Header("Sanity System")]
		[SerializeField] private float minBlurEffect;
		[SerializeField] private float maxBlurEffect;
		[Space(5)]
		[SerializeField] private float minDistortEffect;
		[SerializeField] private float maxDistortEffect;
		[Space(5)]
		[SerializeField] private float minVignetteHorizontalEffect;
		[SerializeField] private float maxVignetteHorizontalEffect;
		[Space(5)]
		[SerializeField] private float minVignetteVerticalEffect;
		[SerializeField] private float maxVignetteVerticalEffect;
		[Space(5)]
		[SerializeField] private Material sanityMaterial;

		private void Awake()
		{
			input = GetComponent<InputManager>();
			playerController = GetComponent<PlayerController>();
		}

		private void Start()
		{ 
			cam = GameObject.FindGameObjectWithTag("CinemachineCam").GetComponent<CinemachineCamera>();
			camNoise = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void Update()
		{
			float mouseX = input.lookInput.x * mouseSensivity * Time.deltaTime;
			float mouseY = input.lookInput.y * mouseSensivity * Time.deltaTime;
			
			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);
			
			cameraPos.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
			
			playerBody.Rotate(Vector3.up * mouseX);
			
			float healthPercent = playerController.GetCurrentHealth() / playerController.GetMaxHealth();
			float intensity = 1f - healthPercent;
			
			camNoise.AmplitudeGain = Mathf.Lerp(minAmplitude, maxAmplitude, intensity);
			camNoise.FrequencyGain = Mathf.Lerp(minFrequency, maxFrequency, intensity);
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



		public void ApplySanityEffect(float sanity)
		{
			sanityMaterial.SetFloat(BlurStrength, Mathf.Lerp(minBlurEffect, maxBlurEffect, 1 - sanity / 100));
			sanityMaterial.SetFloat(DistortStrength, Mathf.Lerp(minDistortEffect, maxDistortEffect, 1 - sanity / 100));
			sanityMaterial.SetFloat(VignetteHorizontalAspect, Mathf.Lerp(minVignetteHorizontalEffect, maxVignetteHorizontalEffect, 1 - sanity / 100));
			sanityMaterial.SetFloat(VignetteVerticalAspect, Mathf.Lerp(minVignetteVerticalEffect, maxVignetteVerticalEffect, 1 - sanity / 100));
		}

		private void OnDestroy()
		{
			sanityMaterial.SetFloat(BlurStrength, minBlurEffect);
			sanityMaterial.SetFloat(DistortStrength, minDistortEffect);
			sanityMaterial.SetFloat(VignetteHorizontalAspect, minDistortEffect);
			sanityMaterial.SetFloat(VignetteVerticalAspect, minDistortEffect);
		}
	}
}
