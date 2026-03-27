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
		
		[Space(20), Header("Debug")]
		[SerializeField] private bool showDebugRay;
		private Vector3 lastRayOrigin;
		private Vector3 lastRayDirection;
		private float lastRayDistance;
		private bool lastRayHit;
		
		private CameraManager cameraManager;


		private void Start()
		{
			cameraManager = GetComponent<CameraManager>();
			
			UpdateUiAmmoInfo();
		}


		public void PickupAmmo(int ammoAmount)
		{
			currentInventoryAmmo += ammoAmount;
			
			UpdateUiAmmoInfo();
		}
		
		
		public void Reload()
		{
			int ammoToReload = maxMagazineAmmo - currentMagazineAmmo;

			if (ammoToReload <= 0) return;

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
			if (currentMagazineAmmo <= 0)
			{
				// Play empty magazine sound?
				return;
			}
			
			if (Time.time >= nextFireTime)
			{
				ShootRaycast();
				UpdateUiAmmoInfo();
				nextFireTime = Time.time + fireCooldown;
			}
		}


		public void ShootRaycast()
		{
			// Play shoot sound
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
	}
}
