using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class InputManager : MonoBehaviour
	{
		private static readonly int IsCrouched = Animator.StringToHash("IsCrouched");

		private PlayerController playerController;
		private PlayerWeapon playerWeapon;
		private PlayerControls inputActions;

		private void Awake()
		{
			playerController = GetComponent<PlayerController>();
			inputActions = new PlayerControls();
			playerWeapon = GetComponent<PlayerWeapon>();
		}

		private void OnEnable()
		{
			inputActions.Player.Enable();
			
			//? Moving
			inputActions.Player.Move.performed += Move;
			inputActions.Player.Move.canceled += Move;
			
			//? Looking
			inputActions.Player.Look.performed += Look;
			inputActions.Player.Look.canceled += Look;
			
			//? Sprint
			inputActions.Player.Sprint.performed += Sprint;
			inputActions.Player.Sprint.canceled += Sprint;
			
			//? Crouch
			inputActions.Player.Crouch.performed += Crouch;
			
			//? Interact
			inputActions.Player.Interact.performed += InteractInput;
			
			//? Aim
			inputActions.Player.Aim.performed += Aim;
			inputActions.Player.Aim.canceled += Aim;
			
			//? Attack
			inputActions.Player.Attack.performed += Attack;
			
			//? Reload
			inputActions.Player.Reload.performed += Reload;
			
			//? Revive
			inputActions.Player.DEBUGRevive.performed += Revive;
			
			//? Healing
			inputActions.Player.Heal.performed += Heal;
		}

		

		private void OnDisable()
		{
			inputActions.Disable();
		}
		
		
		
		public Vector2 moveInput;
		
		public void Move(InputAction.CallbackContext context)
		{
			moveInput = context.ReadValue<Vector2>();
		}

		
		
		public Vector2 lookInput;
		
		public void Look(InputAction.CallbackContext context)
		{
			lookInput = context.ReadValue<Vector2>();
		}



		public bool isSprinting;
		
		public void Sprint(InputAction.CallbackContext context)
		{
			isSprinting = context.ReadValueAsButton();
		}



		public bool isCrouched;
		
		public void Crouch(InputAction.CallbackContext context)
		{
			if (context.action.WasPressedThisFrame() && playerController.canAttack)
			{
				isCrouched = !isCrouched;
				GetComponent<Animator>().SetBool(IsCrouched, isCrouched);
			}
		}



		public void InteractInput(InputAction.CallbackContext context)
		{
			if (context.action.WasPressedThisFrame())
			{
				playerController.Interact();
			}
		}



		public bool isAiming;
		
		public void Aim(InputAction.CallbackContext context)
		{
			if (!playerController.canAttack) return;
			isAiming = context.action.IsPressed();
		}


		
		public void Attack(InputAction.CallbackContext context)
		{
			if (isAiming)
				playerWeapon.Shoot();
			else
				playerController.PerformAttack();
		}



		public void Reload(InputAction.CallbackContext context)
		{
			playerWeapon.Reload();
		}



		public void Revive(InputAction.CallbackContext context)
		{
			playerController.Revive();
		}
		
		
		
		private void Heal(InputAction.CallbackContext context)
		{
			playerController.UseHealthPack();
		}
	}
}
