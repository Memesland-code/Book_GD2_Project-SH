using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class InputManager : MonoBehaviour
	{
		private static readonly int IsCrouched = Animator.StringToHash("IsCrouched");

		private PlayerController playerController;
		private PlayerControls inputActions;

		private void Awake()
		{
			playerController = GetComponent<PlayerController>();
			inputActions = new PlayerControls();
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
			inputActions.Player.Interact.performed += Interact;
			
			//? Aim
			inputActions.Player.Aim.performed += Aim;
			inputActions.Player.Aim.canceled += Aim;
			
			//? Shoot
			inputActions.Player.Shoot.performed += Shoot;
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
			if (context.action.WasPressedThisFrame())
			{
				isCrouched = !isCrouched;
				GetComponent<Animator>().SetBool(IsCrouched, isCrouched);
			}
		}



		public bool hasInteracted;
		
		public void Interact(InputAction.CallbackContext context)
		{
			hasInteracted = context.action.WasPressedThisFrame();
			print(hasInteracted);
		}



		public bool isAiming;
		
		public void Aim(InputAction.CallbackContext context)
		{
			isAiming = context.action.IsPressed();
		}


		
		public void Shoot(InputAction.CallbackContext context)
		{
			if (isAiming)
				playerController.Shoot();
			
		}
	}
}
