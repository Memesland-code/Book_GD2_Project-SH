using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
	public class PlayerWeapon : MonoBehaviour
	{
		[Header("Required info")]
		[SerializeField] private Transform muzzle;
		[SerializeField] private LayerMask shootableLayers;
		[SerializeField] private float weaponRange;
		[SerializeField] private float weaponDamage;
		
		[Header("Gameplay constraints")]
		[SerializeField] private int maxMagazineAmmo;
		private int currentMagazineAmmo;
		[SerializeField] private int currentInventoryAmmo;
		
		[Space(10)]
		[SerializeField] private float reloadTime;
		[SerializeField] private float fireCooldown;
		private float nextFireTime;
		
		[Header("Sound System")]
		[SerializeField] private float fireHearRadius;
		[SerializeField] private AudioSource weaponAudioSource;
		[SerializeField] private AudioClip fireSound;
		[SerializeField] private AudioClip emptyMagazineSound;
		[SerializeField] private AudioClip reloadSound;
		
		[Header("VFX")]
		[SerializeField] private GameObject flashVFX;
		[SerializeField] private ParticleSystem flashParticle;
		[SerializeField] private Light flashLight;
		[SerializeField] private float flashDuration = 0.03f;
		
		[Header("Recoil")]
		[SerializeField] private Transform armsPivot;
		[SerializeField] private float kickbackAmount;
		[SerializeField] private float returnSpeed;
		[SerializeField] private float snappiness;
		[SerializeField] private CinemachineImpulseSource impulseSource;

		private Vector3 targetPosition;
		private Vector3 currentPosition;
		private Vector3 initialPosition;
		
		[Space(20), Header("Debug")]
		[SerializeField] private bool showDebugRay;
		private Vector3 lastRayOrigin;
		private Vector3 lastRayDirection;
		private float lastRayDistance;
		private bool lastRayHit;
		
		private PlayerController playerController;
		private CameraManager cameraManager;


		private void Start()
		{
			playerController = GetComponent<PlayerController>();
			cameraManager = GetComponent<CameraManager>();
			
			UpdateUiAmmoInfo();

			initialPosition = armsPivot.localPosition;
		}


		private void Update()
		{
			// Recoil - Return to base position
			targetPosition = Vector3.Lerp(targetPosition, initialPosition, returnSpeed * Time.deltaTime);
			currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.deltaTime);
			armsPivot.localPosition = currentPosition;
		}


		public void PickupAmmo(int ammoAmount)
		{
			currentInventoryAmmo += ammoAmount;
			
			UpdateUiAmmoInfo();
		}
		
		
		public void Reload()
		{
			if (!playerController.canAttack) return;

			if (currentInventoryAmmo == 0)
			{
				weaponAudioSource.clip = emptyMagazineSound;
				weaponAudioSource.Play();
				return;
			}
			
			int ammoToReload = maxMagazineAmmo - currentMagazineAmmo;

			if (ammoToReload <= 0) return;
			
			weaponAudioSource.clip = reloadSound;
			weaponAudioSource.Play();

			if (currentInventoryAmmo >= ammoToReload)
			{
				currentMagazineAmmo = maxMagazineAmmo;
				currentInventoryAmmo -= ammoToReload;
			}
			else
			{
				currentMagazineAmmo += currentInventoryAmmo;
				currentInventoryAmmo = 0;
			}
			
			UpdateUiAmmoInfo();
		}
		
		
		public void Shoot()
		{
			if (!playerController.canAttack) return;
			
			if (currentMagazineAmmo <= 0)
			{
				weaponAudioSource.clip = emptyMagazineSound;
				weaponAudioSource.Play();
				return;
			}
			
			if (Time.time >= nextFireTime)
			{
				StartCoroutine(ShootFlash());
				FireRecoil();
				ShootRaycast();
				UpdateUiAmmoInfo();
				nextFireTime = Time.time + fireCooldown;
			}
		}


		private IEnumerator ShootFlash()
		{
			flashVFX.SetActive(true);
			if (flashParticle) flashParticle.Play();
			if (flashLight) flashLight.enabled = true;
			
			yield return new WaitForSeconds(flashDuration);
			
			if (flashLight) flashLight.enabled = false;
			flashVFX.SetActive(false);
		}


		private void FireRecoil()
		{
			// Moving arms
			targetPosition -= new Vector3(0, 0, kickbackAmount);
			impulseSource.GenerateImpulse();
		}


		public void ShootRaycast()
		{
			weaponAudioSource.clip = fireSound;
			weaponAudioSource.Play();
			// Play muzzle flash VFX
			
			SoundSystem.EmitSound(muzzle.position, fireHearRadius, gameObject);

			Vector3 origin = cameraManager.cam.transform.position;
			Vector3 direction = cameraManager.cam.transform.forward;
			
			lastRayOrigin = origin;
			lastRayDirection = direction;
			lastRayDistance = weaponRange;
			
			Ray ray = new Ray(origin, direction);

			if (Physics.Raycast(ray, out RaycastHit hit, weaponRange, shootableLayers))
			{
				lastRayDistance = hit.distance;
				lastRayHit = true;
				
				if (hit.collider.gameObject.TryGetComponent(out IDamageable damageable))
				{
					damageable.TakeDamage(weaponDamage, gameObject);
				}
			}
			else
			{
				lastRayHit = false;
			}

			currentMagazineAmmo--;
		}


		private void UpdateUiAmmoInfo()
		{
			GameManager.Instance.GetWeaponUi().SetAmmoText(currentMagazineAmmo, currentInventoryAmmo);
		}

		public int GetTotalAmmo()
		{
			return currentMagazineAmmo + currentInventoryAmmo;
		}

		private void OnDrawGizmos()
		{
			if (!showDebugRay) return;
			
			Gizmos.color = lastRayHit ? Color.green : Color.red;
			
			Vector3 endPoint = lastRayOrigin + lastRayDirection * lastRayDistance;
			
			Gizmos.DrawLine(lastRayOrigin, endPoint);

			if (lastRayHit)
			{
				Gizmos.color = Color.darkOrange;
				Gizmos.DrawSphere(endPoint, 0.05f);
			}
			
			Gizmos.color = Color.yellowNice;
			Gizmos.DrawWireSphere(muzzle.position, fireHearRadius);
		}
		
		//* ===== DEMO ONLY =====

		public void RemoveAllAmmo()
		{
			currentMagazineAmmo = 0;
			currentInventoryAmmo = 0;
			UpdateUiAmmoInfo();
		}
	}
}
