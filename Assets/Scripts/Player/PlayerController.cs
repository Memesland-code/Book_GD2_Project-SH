using System.Collections;
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

		private InputManager input;
	
		[Header("Movement")]
		[SerializeField] private float baseSpeed;
		[SerializeField] private float sprintSpeed;
		[SerializeField] private float crouchSpeed;
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
		
		[Space(10)]
		[SerializeField] private GameObject charBody;
		[SerializeField] private GameObject charEyeLashes;
		[SerializeField] private GameObject charHair;
		[SerializeField] private GameObject charHoody;
		[SerializeField] private GameObject charPants;
		
		private Rigidbody rb;
		private Animator animator;
		private CapsuleCollider playerCollider;
		private CameraManager cameraManager;

		[SerializeField] private Vector3 standCameraPosition = new (0, 1.65f, 0.15f);
		[SerializeField] private Vector3 crouchedCameraPosition = new (0, 0.79f, 0.4f);
		
		private Vector3 playerVelocity;
		private bool isDead;
		private Vector3 playerSpawnPosition;
		private Vector3 playerSpawnRotation;
	
		void Awake()
		{
			input = GetComponent<InputManager>();
			rb = GetComponent<Rigidbody>();
			animator = GetComponent<Animator>();
			playerCollider = GetComponent<CapsuleCollider>();
			cameraManager = GetComponent<CameraManager>();
			
			playerSpawnPosition = rb.transform.position;
			playerSpawnRotation = rb.transform.rotation.eulerAngles;
		}

		private void Start()
		{
			arms.SetActive(false);
			
			currentSpeed = baseSpeed;
			
			currentHealth = maxHealth;
			isDead = false;

			standCenter = playerCollider.center;
			standHeight = playerCollider.height;
		}

		private void Update()
		{
			UpdateMoveStatus();
			UpdatePlayerColliderAndCam();
			
			cameraManager.SetAiming(input.isAiming);
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
				currentSpeed /= backwardsMoveSpeedMultiplier;
			}

			if (input.isAiming)
			{
				currentSpeed /= aimMoveSpeedMultiplier;
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
		
		public void TakeDamage(float damageAmount, GameObject damageSource)
		{
			if (isDead || Time.time <= nextDamageAcceptTime) return;

			nextDamageAcceptTime = Time.time + damageCooldown;
			
			currentHealth -= damageAmount;

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
			yield return new WaitForSeconds(2.5f);
			GameManager.Instance.ManagePlayerDeath();
			gameObject.SetActive(false);
		}

		public void Heal(float healAmount)
		{
			currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, 100);
		}

		public void Revive()
		{
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;
			transform.position = playerSpawnPosition;
			transform.rotation = Quaternion.Euler(playerSpawnRotation);
			rb.isKinematic = false;
			
			animator.SetTrigger(RevivePlayer);
			GetComponent<InputManager>().enabled = true;
		}

		public void RunTriggerDetection(Collider otherCollider)
		{
			throw new System.NotImplementedException();
		}
	}
}
