using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class InputManager : MonoBehaviour
	{
		
		[Header("Input Actions")]
		public InputActionReference moveAction;
		public InputActionReference lookAction;

		private void OnEnable()
		{
			moveAction.action.Enable();
			lookAction.action.Enable();
		}

		private void OnDisable()
		{
			moveAction.action.Disable();
			lookAction.action.Disable();
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
	}
}
