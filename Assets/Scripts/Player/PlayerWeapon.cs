using System;
using UnityEngine;

namespace Player
{
	public class PlayerWeapon : MonoBehaviour
	{
		[Header("Required info")]
		[SerializeField] private Transform muzzle;
		[SerializeField] private LayerMask shootableLayers;
		[SerializeField] private float weaponRange;
		
		[Header("Gameplay constraints")]
		[SerializeField] private int maxMagazineAmmo;
		private int currentMagazineAmmo;
		[SerializeField] private int currentInventoryAmmo;
		
		[Space(10)]
		[SerializeField] private float reloadTime;
		[SerializeField] private float fireCooldown;
		private float nextFireTime;
		
		[Space(20), Header("Debug")]
		[SerializeField] private bool showDebugRay;
		private Vector3 lastRayOrigin;
		private Vector3 lastRayDirection;
		private float lastRayDistance;
		private bool lastRayHit;


		public void PickupAmmo(int ammoAmount)
		{
			currentInventoryAmmo += ammoAmount;
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
		}
		
		
		public void Shoot()
		{
			if (Time.time >= nextFireTime)
			{
				ShootRaycast();
				nextFireTime = Time.time + fireCooldown;
			}
		}


		public void ShootRaycast()
		{
			Vector3 origin = muzzle.position;
			Vector3 direction = muzzle.forward;
			
			lastRayOrigin = origin;
			lastRayDirection = direction;
			lastRayDistance = weaponRange;
			
			Ray ray = new Ray(origin, direction);

			if (Physics.Raycast(ray, out RaycastHit hit, weaponRange, shootableLayers))
			{
				lastRayDistance = hit.distance;
				lastRayHit = true;
				
				//! Hit enemies
			}
			else
			{
				lastRayHit = false;
			}

			currentMagazineAmmo--;
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
		}
	}
}
