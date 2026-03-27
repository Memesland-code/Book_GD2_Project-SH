using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
	public class PlayerController : MonoBehaviour, IDamageable
	{
		private static readonly int MoveX = Animator.StringToHash("MoveX");
		private static readonly int MoveY = Animator.StringToHash("MoveY");
		private static readonly int IsCrouched = Animator.StringToHash("IsCrouched");
		private static readonly int Death = Animator.StringToHash("Death");
		private static readonly int RevivePlayer = Animator.StringToHash("RevivePlayer");
		private static readonly int Stab = Animator.StringToHash("Stab");
		private static readonly int Punch = Animator.StringToHash("Punch");
	
		[Header("Movement")]
		[SerializeField] private float baseSpeed;
		[SerializeField] private float sprintSpeed;
		[SerializeField] private float crouchSpeed;
		[SerializeField] private float runningSoundRadius;
		private float currentSpeed;
		
		[Header("Crouching")]
		[SerializeField] private float crouchHeight;
		[SerializeField] private Vector3 crouchCenter;
		[SerializeField] private float crouchTransitionSpeed;
		private float standHeight;
		private Vector3 standCenter;
		
		[Header("Life system")]
		[SerializeField] private float maxHealth = 100;
		private float currentHealth;
		[SerializeField] private float damageCooldown;
		private float nextDamageAcceptTime;
		
		[Header("Shooting system")]
		[SerializeField] private GameObject arms;
		[SerializeField] private float backwardsMoveSpeedMultiplier;
		[SerializeField] private float aimMoveSpeedMultiplier;
		
		[Header("AttackSystem")]
		[SerializeField] private GameObject attackPoint;
		[SerializeField] float punchDamage;
		[SerializeField] float stabDamage;
		public bool canAttack = true;
		private bool isPrimaryAttackWay = true; // True = punch | False = stab
		
		[Space(10)]
		[SerializeField] private GameObject charBody;
		[SerializeField] private GameObject charEyeLashes;
		[SerializeField] private GameObject charHair;
		[SerializeField] private GameObject charHoody;
		[SerializeField] private GameObject charPants;
		
		[Header("Inventory System")]
		public List<HealthPackInstance> healthPacks = new List<HealthPackInstance>();
		private float totalPotentialHealth;
		
		public List<KnifeInstance> knives = new List<KnifeInstance>();
		public KnifeInstance currentKnife => knives.LastOrDefault(k => k.IsUsable);
		
		[Header("Interaction System")]
		[SerializeField] private float interactDistance;
		[SerializeField] private float interactSphereRadius;
		[SerializeField] private LayerMask pickableLayer;
		
		private InputManager input;
		private Rigidbody rb;
		private Animator animator;
		private CapsuleCollider playerCollider;
		private CameraManager cameraManager;
		[HideInInspector] public PlayerWeapon playerWeapon;

		[SerializeField] private Vector3 standCameraPosition = new (0, 1.65f, 0.15f);
		[SerializeField] private Vector3 crouchedCameraPosition = new (0, 0.79f, 0.4f);
		
		private Vector3 playerVelocity;
		private bool isDead;
		private Vector3 playerSpawnPosition;
		private Vector3 playerSpawnRotation;
		
		[Space(10), Header("DEBUG")]
		[SerializeField] bool showDebugGizmos;
		private Vector3 interactSphereOrigin;
	
		void Awake()
		{
			input = GetComponent<InputManager>();
			rb = GetComponent<Rigidbody>();
			animator = GetComponent<Animator>();
			playerCollider = GetComponent<CapsuleCollider>();
			cameraManager = GetComponent<CameraManager>();
			playerWeapon = GetComponent<PlayerWeapon>();
			
			playerSpawnPosition = rb.transform.position;
			playerSpawnRotation = rb.transform.rotation.eulerAngles;
			
			attackPoint.GetComponent<Collider>().enabled = false;
		}

		private void Start()
		{
			arms.SetActive(false);
			
			currentSpeed = baseSpeed;
			
			currentHealth = maxHealth;
			isDead = false;

			standCenter = playerCollider.center;
			standHeight = playerCollider.height;
			
			RefreshKnifeVisuals();
			GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(healthPacks.Count, (int)(healthPacks.FirstOrDefault()?.HealAmount ?? 0));
		}

		//* ===== Update Logic =====
		private void Update()
		{
			UpdateMoveStatus();
			UpdatePlayerColliderAndCam();
			
			cameraManager.SetCameraFOV(input.isAiming, isDead);
			if (input.isAiming)
			{
				arms.SetActive(true);
				charBody.SetActive(false);
				charHoody.SetActive(false);
				charPants.SetActive(false);
			}
			else if (input.isCrouched)
			{
				arms.SetActive(false);
				charBody.SetActive(false);
				charEyeLashes.SetActive(false);
				charHair.SetActive(false);
				charHoody.SetActive(false);
			}
			else
			{
				arms.SetActive(false);
				charBody.SetActive(true);
				charEyeLashes.SetActive(true);
				charHair.SetActive(true);
				charHoody.SetActive(true);
				charPants.SetActive(true);
			}
		}

		private void UpdateMoveStatus()
		{
			if (input.isSprinting)
			{
				SoundSystem.EmitSound(transform.position, runningSoundRadius, gameObject);
				input.isCrouched = false;
				currentSpeed = sprintSpeed;
				animator.SetBool(IsCrouched, false);
				animator.SetFloat(MoveX, input.moveInput.x + 0.25f); // Virtually sets animator to running state
				animator.SetFloat(MoveY, input.moveInput.y + 0.25f);
			}
			else if (input.isCrouched)
			{
				currentSpeed = crouchSpeed;
				animator.SetBool(IsCrouched, true);
				animator.SetFloat(MoveX, input.moveInput.x);
				animator.SetFloat(MoveY, input.moveInput.y);
			}
			else
			{
				currentSpeed = baseSpeed;
				animator.SetFloat(MoveX, input.moveInput.x);
				animator.SetFloat(MoveY, input.moveInput.y);
			}

			if (input.moveInput.y < 0)
			{
				currentSpeed *= backwardsMoveSpeedMultiplier;
			}

			if (input.isAiming)
			{
				currentSpeed *= aimMoveSpeedMultiplier;
			}
		}

		private void UpdatePlayerColliderAndCam()
		{
			Vector3 targetCenter = standCenter;
			float targetHeight = standHeight;
			Vector3 targetCameraPosition = standCameraPosition;
			
			if (input.isCrouched)
			{
				targetCameraPosition = crouchedCameraPosition;
				targetCenter = crouchCenter;
				targetHeight = crouchHeight;
			}

			cameraManager.SetCameraPosition(input.isCrouched, targetCameraPosition, crouchTransitionSpeed);
			playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
			playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, crouchTransitionSpeed * Time.deltaTime);
		}

		private void FixedUpdate()
		{
			Vector3 direction = transform.right * input.moveInput.x + transform.forward * input.moveInput.y;
			direction.Normalize();
			rb.linearVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
		}
		
		
		
		//* ===== IDamageable Systems =====
		public void TakeDamage(float damageAmount, GameObject damageSource)
		{
			if (isDead || Time.time <= nextDamageAcceptTime) return;

			nextDamageAcceptTime = Time.time + damageCooldown;
			
			currentHealth -= damageAmount;
			
			GameManager.Instance.GetPlayerLifeUI().SetPlayerLifePercent(currentHealth/maxHealth);

			if (currentHealth <= 0)
			{
				animator.SetTrigger(Death);
				StartCoroutine(PlayerDeath());
			}
		}

		
		private IEnumerator PlayerDeath()
		{
			isDead = true;
			GetComponent<InputManager>().enabled = false;
			input.isAiming = false;
			input.isCrouched = false;
			animator.SetBool(IsCrouched, false);
			yield return new WaitForSeconds(2.5f);
			GameManager.Instance.ManagePlayerDeath();
			gameObject.SetActive(false);
		}

		
		public void Heal(float healAmount)
		{
			currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, 100);
			GameManager.Instance.GetPlayerLifeUI().SetPlayerLifePercent(currentHealth/maxHealth);
		}

		
		public void Revive()
		{
			Start();
			
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;
			transform.position = playerSpawnPosition;
			transform.rotation = Quaternion.Euler(playerSpawnRotation);
			rb.isKinematic = false;
			
			animator.SetTrigger(RevivePlayer);
			GetComponent<InputManager>().enabled = true;
		}
		
		
		
		//* ===== Attack system =====
		// Attack when input detected
		public void PerformAttack()
		{
			if (!canAttack) return;

			canAttack = false;
			input.isCrouched = false;
			animator.SetBool(IsCrouched, false);
			
			if (knives.Count > 0)
			{
				isPrimaryAttackWay = false;
				animator.SetTrigger(Stab);
			}
			else
			{
				isPrimaryAttackWay = true;
				animator.SetTrigger(Punch);
			}
		}
		
		
		// Callback from animation: enable hit collision
		public void EnableAttackCollider(int isStart)
		{
			GetComponentInChildren<HitDetectZone>().gameObject.GetComponent<BoxCollider>().enabled = isStart == 1;
		}
		
		// Called from collision if triggerEnter detected
		public void OnAttackCollision(Collider otherCollider)
		{
			if (otherCollider.TryGetComponent(out IDamageable damageable))
			{
				if (isPrimaryAttackWay)
				{
					damageable.TakeDamage(punchDamage, gameObject);
				}
				else
				{
					damageable.TakeDamage(stabDamage, gameObject);
				}
			}
		}

		public void DamageReceived()
		{
			if (!isPrimaryAttackWay) currentKnife.Use();
		}

		// Called on animatiion end to reset attack state
		public void ResetAttack()
		{
			canAttack = true;
		}
		
		
		
		//* ===== Interactions and inventory =====
		public void Interact()
		{
			Ray ray = new Ray(cameraManager.cam.transform.position, cameraManager.cam.transform.forward);

			Vector3 sphereOrigin = ray.GetPoint(interactDistance);
			interactSphereOrigin = sphereOrigin;
			
			if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
			{
				if (hit.collider.TryGetComponent(out IPickable pickable))
				{
					pickable.OnPickUp(this);
					return;
				}
				
				sphereOrigin = hit.point;
				interactSphereOrigin = sphereOrigin;
			}
			
			Collider[] hits = Physics.OverlapSphere(sphereOrigin, interactSphereRadius, pickableLayer);

			if (hits.Length > 0)
			{
				Collider closest = hits
					.OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
					.First();

				if (closest.TryGetComponent(out IPickable spherePickable))
				{
					spherePickable.OnPickUp(this);
				}
			}
		}



		public void AddKnife(ItemData data, float durability)
		{
			var knife = new KnifeInstance { Data = data, Durability = durability };

			// Create instance and set break event to execute the remove knife from inventory
			knife.OnBroken += () =>
			{
				knives.Remove(knife);
				RefreshKnifeVisuals();
			};
			
			knives.Add(knife);
			RefreshKnifeVisuals();
			
		}
		
		private void RefreshKnifeVisuals()
		{
			KnifeUI knifeUI = GameManager.Instance.GetKnifeUI();
			knifeUI.RefreshKnivesUI(knives.Count, currentKnife?.Durability ?? 0);
			
			if (knives.Count > 0 && currentKnife != null)
			{
				attackPoint.GetComponent<MeshFilter>().mesh = currentKnife.Data.mesh;
				attackPoint.GetComponent<MeshRenderer>().materials = currentKnife.Data.materials;
			}
			else
			{
				attackPoint.GetComponent<MeshFilter>().mesh = null;
			}
		}



		public void AddHealthPack(ItemData data, float healAmount)
		{
			var pack = new HealthPackInstance { Data = data, HealAmount = healAmount };

			pack.OnUsed += () => healthPacks.Remove(pack);
			healthPacks.Add(pack);
			
			var firstPack = healthPacks.FirstOrDefault();
			
			if (firstPack != null)
				GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(healthPacks.Count, (int)firstPack.HealAmount);
		}

		public void UseHealthPack()
		{
			var pack = healthPacks.FirstOrDefault();
			if (pack != null)
			{
				pack.Use(this);
				GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(healthPacks.Count, (int)(healthPacks.FirstOrDefault()?.HealAmount ?? 0));
			}

		}

		
		
		//* ===== Other =====
		public (float, int) GetPotentialHealthAndAmmo()
		{
			totalPotentialHealth = 0;
			
			foreach (HealthPackInstance healthPack in healthPacks)
			{
				totalPotentialHealth += healthPack.HealAmount;
			}
			
			return (currentHealth + totalPotentialHealth, playerWeapon.GetTotalAmmo());
		}
		
		
		
		//* ===== Debug =====
		private void OnDrawGizmos()
		{
			if (!showDebugGizmos) return;

			if (Application.isPlaying)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawRay(cameraManager.cam.transform.position, cameraManager.cam.transform.forward);
			}

			Gizmos.color = Color.rebeccaPurple;
			Gizmos.DrawWireSphere(interactSphereOrigin, interactSphereRadius);

			Gizmos.color = Color.darkOrange;
			Gizmos.DrawWireSphere(transform.position, runningSoundRadius);
		}
	}
}
