using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	public float MovX;
	public float MovY;

	public void OnMove(InputAction.CallbackContext context)
	{
		Vector2 moveVector = context.ReadValue<Vector2>();
		MovX = moveVector.x; 
		MovY = moveVector.y; 
	}
}
