using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

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
		[SerializeField] private float punchDamage;
		[SerializeField] public float stabDamage;
		public bool canAttack = true;
		private bool isPrimaryAttackWay = true; // True = punch | False = stab
		
		[Space(10)]
		[SerializeField] private GameObject charBody;
		[SerializeField] private GameObject charEyeLashes;
		[SerializeField] private GameObject charHair;
		[SerializeField] private GameObject charHoody;
		[SerializeField] private GameObject charPants;
		
		[Header("Inventory System")]
		public List<HealthPackInstance> HealthPacks = new List<HealthPackInstance>();
		private float totalPotentialHealth;
		
		public List<KnifeInstance> Knives = new List<KnifeInstance>();
		public KnifeInstance CurrentKnife => Knives.LastOrDefault(k => k.IsUsable);
		
		[Header("Interaction System")]
		[SerializeField] private float interactDistance;
		[SerializeField] private float interactSphereRadius;
		[SerializeField] private LayerMask interactableLayers;
		
		[Header("Sanity System")]
		[SerializeField] private float maxSanity;
		private float currentSanity;
		[SerializeField, Tooltip("In point / second")] private float sanityMaxDrainSpeed;
		[SerializeField, Tooltip("In point / second")] private float recoveryRate;
		[Space(5)]
		[SerializeField] private LayerMask horrorLayer;
		[SerializeField] private LayerMask obstructionLayer;
		[SerializeField] private float viewDistance;
		[SerializeField, Range(0f, 120f)] private float maxPeripheralConeAngle;
		[SerializeField, Range(0f, 120f)] private float minPeripheralConeAngle;
		private float currentPeripheralConeAngle;
		private float peripheralLookThreshold;

		[Header("SoundSystem")]
		[SerializeField] private AudioMixer mainMixer;
		[SerializeField] private AudioSource weaponAudioSource;
		[SerializeField] private AudioClip stabSound;
		
		[Header("HeartBeat effect")]
		[SerializeField] private AudioSource heartBeatSound;
		private const string SanityLowPassParam = "SanityLowPass";
		[SerializeField] float normalFrequency = 22000;
		[SerializeField] float muffledFrequency = 500;
		
		[SerializeField] private float minPitch;
		[SerializeField] private float maxPitch;
		[SerializeField] private float maxVolume;
		
		private InputManager input;
		private Rigidbody rb;
		private Animator animator;
		private CapsuleCollider playerCollider;
		private CameraManager cameraManager;
		[HideInInspector] public PlayerWeapon playerWeapon;

		[Header("Camera")]
		[SerializeField] private Vector3 standCameraPosition = new (0, 1.65f, 0.15f);
		[SerializeField] private Vector3 crouchedCameraPosition = new (0, 0.79f, 0.4f);
		
		private Vector3 playerVelocity;
		private bool isDead;
		private Vector3 playerSpawnPosition;
		private Vector3 playerSpawnRotation;
		
		[Space(10), Header("DEBUG")]
		[SerializeField] bool showDebugGizmos;
		[SerializeField] bool showDirectLookCone;
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

			currentSanity = maxSanity;

			standCenter = playerCollider.center;
			standHeight = playerCollider.height;
			
			currentPeripheralConeAngle = maxPeripheralConeAngle;
			
			RefreshKnifeVisuals();
			GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(HealthPacks.Count, (int)(HealthPacks.FirstOrDefault()?.HealAmount ?? 0));
			GameManager.Instance.GetPlayerLifeUI().SetPlayerLifePercent(currentHealth/maxHealth);

			if (heartBeatSound) heartBeatSound.volume = 0;
		}

		//* ===== Update Logic =====
		private void Update()
		{
			UpdateMoveStatus();
			UpdatePlayerColliderAndCam();
			UpdateSanitySystem();
			
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
				animator.SetFloat(MoveX, input.moveInput.x + 0.25f);
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
		public void TakeDamage(float damageAmount, GameObject damageSource, bool ignoreCooldown)
		{
			if ((isDead || Time.time <= nextDamageAcceptTime) && !ignoreCooldown) return;

			nextDamageAcceptTime = Time.time + damageCooldown;
			
			currentHealth -= damageAmount;
			
			GameManager.Instance.GetPlayerLifeUI().SetPlayerLifePercent(currentHealth/maxHealth);

			if (currentHealth <= 0)
			{
				if (damageSource.GetComponent<IInteractable>() != null) currentHealth = 1;
				
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
			
			if (Knives.Count > 0)
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
		public void OnAttackCollision(Collider otherCollider, bool isRadialAttack)
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

		public void DamageReceivedByTarget(bool isKnifeAttack)
		{
			if (!isPrimaryAttackWay && isKnifeAttack)
			{
				weaponAudioSource.clip = stabSound;
				weaponAudioSource.Play();
				CurrentKnife.Use();
				RefreshKnifeVisuals();
			}
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

				if (hit.collider.TryGetComponent(out IInteractable interactable))
				{
					interactable.Interact(gameObject);
					return;
				}
				
				sphereOrigin = hit.point;
				interactSphereOrigin = sphereOrigin;
			}
			
			Collider[] hits = Physics.OverlapSphere(sphereOrigin, interactSphereRadius, interactableLayers);

			if (hits.Length > 0)
			{
				Collider closest = hits
					.OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
					.First();

				if (closest.TryGetComponent(out IPickable spherePickable))
				{
					spherePickable.OnPickUp(this);
				}
				
				if (closest.TryGetComponent(out IInteractable interactable))
				{
					interactable.Interact(gameObject);
				}
			}
		}



		public void AddKnife(ItemData data, float durability)
		{
			var knife = new KnifeInstance { Data = data, Durability = durability };

			// Create instance and set break event to execute the remove knife from inventory
			knife.OnBroken += () =>
			{
				Knives.Remove(knife);
				RefreshKnifeVisuals();
			};
			
			Knives.Add(knife);
			RefreshKnifeVisuals();
		}
		
		private void RefreshKnifeVisuals()
		{
			KnifeUI knifeUI = GameManager.Instance.GetKnifeUI();
			knifeUI.RefreshKnivesUI(Knives.Count, CurrentKnife?.Durability ?? 0);
			
			if (Knives.Count > 0 && CurrentKnife != null)
			{
				attackPoint.GetComponent<MeshFilter>().mesh = CurrentKnife.Data.mesh;
				attackPoint.GetComponent<MeshRenderer>().materials = CurrentKnife.Data.materials;
			}
			else
			{
				attackPoint.GetComponent<MeshFilter>().mesh = null;
			}
		}



		public void AddHealthPack(ItemData data, float healAmount)
		{
			var pack = new HealthPackInstance { Data = data, HealAmount = healAmount };

			pack.OnUsed += () => HealthPacks.Remove(pack);
			HealthPacks.Add(pack);
			
			var firstPack = HealthPacks.FirstOrDefault();
			
			if (firstPack != null)
				GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(HealthPacks.Count, (int)firstPack.HealAmount);
		}

		public void UseHealthPack()
		{
			if (currentHealth == maxHealth) return;
			
			var pack = HealthPacks.FirstOrDefault();
			if (pack != null)
			{
				pack.Use(this);
				GameManager.Instance.GetHealthPacksUI().SetHealthPacksInfo(HealthPacks.Count, (int)(HealthPacks.FirstOrDefault()?.HealAmount ?? 0));
			}
		}
		
		
		
		//* ===== Sanity System =====
		private void UpdateSanitySystem()
		{
			peripheralLookThreshold = Mathf.Cos(currentPeripheralConeAngle * Mathf.Deg2Rad);
			float highestInfluence = 0;
			Transform cam = cameraManager.cam.transform;

			// Scanning zone
			Collider[] hits = Physics.OverlapSphere(cam.position, viewDistance, horrorLayer);
			
			foreach (var hit in hits)
			{
				Vector3 dirToTarget = (hit.bounds.center - cam.position).normalized;
				
				float dot = Vector3.Dot(cam.forward, dirToTarget);
				
				// Checking if object in angle
				if (dot < peripheralLookThreshold) continue;
				
				float distToHit = Vector3.Distance(cam.position, hit.bounds.center);
				if (Physics.Raycast(cam.position, dirToTarget, distToHit, obstructionLayer)) continue;
				
				// Calculates the most centered-point to determine the highest horroro in-view object
				float angleToCenter = (dot - peripheralLookThreshold) / (1f - peripheralLookThreshold);
				highestInfluence = Mathf.Max(highestInfluence, Mathf.Lerp(0f, sanityMaxDrainSpeed, angleToCenter));
			}
			
			// Updating sanity value
			currentSanity += (highestInfluence > 0f ? -highestInfluence : recoveryRate) * Time.deltaTime;
			currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
			currentPeripheralConeAngle = Mathf.Lerp(minPeripheralConeAngle, maxPeripheralConeAngle, currentSanity / 100);
			
			cameraManager.ApplySanityEffect(currentSanity);
			
			UpdateHeartBeatEffect();
		}



		private void UpdateHeartBeatEffect()
		{
			// Revert sanity value: 0 = good, 100 = bad - easier to calculate after
			float t = 1f - (currentSanity / 100);

			float targetFreqency = Mathf.Lerp(normalFrequency, muffledFrequency, t);
			mainMixer.SetFloat(SanityLowPassParam, targetFreqency);

			if (heartBeatSound)
			{
				float soundThreshold = Mathf.Clamp01((t - 0.3f) / 0.9f);
				heartBeatSound.volume = soundThreshold * maxVolume;

				heartBeatSound.pitch = Mathf.Lerp(minPitch, maxPitch, t);
			}
		}

		
		
		//* ===== Other =====
		public (float, int) GetPotentialHealthAndAmmo()
		{
			totalPotentialHealth = 0;
			
			foreach (HealthPackInstance healthPack in HealthPacks)
			{
				totalPotentialHealth += healthPack.HealAmount;
			}
			
			return (currentHealth + totalPotentialHealth, playerWeapon.GetTotalAmmo());
		}

		public float GetCurrentHealth()
		{
			return currentHealth;
		}

		public float GetMaxHealth()
		{
			return maxHealth;
		}
		
		
		
		//* ===== Debug =====
		private void OnDrawGizmos()
		{
			if (!showDebugGizmos) return;

			if (Application.isPlaying)
			{
				Gizmos.color = Color.tomato;
				DrawCone(cameraManager.cam.transform.position, cameraManager.cam.transform.forward, currentPeripheralConeAngle, viewDistance);
			}

			Gizmos.color = new Color(1, 0, 0, 0.9f);
			Gizmos.DrawWireSphere(transform.position, viewDistance);
			
			Gizmos.color = Color.rebeccaPurple;
			Gizmos.DrawWireSphere(interactSphereOrigin, interactSphereRadius);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, runningSoundRadius);
		}

		private void DrawCone(Vector3 origin, Vector3 forward, float angle, float range)
		{
			int segments = 20;
			Vector3 prevPoint = Vector3.zero;

			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments * 360f * Mathf.Deg2Rad;
				
				Vector3 right = cameraManager.cam.transform.right;
				Vector3 up = cameraManager.cam.transform.up;
				float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * range;

				Vector3 point = origin + forward * range
				                       + right * (Mathf.Cos(t) * radius)
				                       + up * (Mathf.Sin(t) * radius);

				if (i > 0)
				{
					Gizmos.DrawLine(prevPoint, point);
					if (i % 5 == 0) Gizmos.DrawLine(origin, point);
				}

				prevPoint = point;
			}
		}
		
		
		//* ===== DEMO ONLY =====

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out CheckPoint checkpoint))
			{
				playerSpawnPosition = checkpoint.GetCheckpointPosition();
			}
		}

		public void SetCurrentHealth(float newHealth)
		{
			currentHealth = Mathf.Clamp(newHealth, 1, 100);
		}
	}
}
