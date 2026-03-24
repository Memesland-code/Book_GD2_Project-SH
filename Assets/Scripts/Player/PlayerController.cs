using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerControler : MonoBehaviour
	{
		private InputManager inputs;
	
		[Header("Movement")]
		[SerializeField] private float speed;
		
		private float gravityValue = -9.81f;
		
		private Rigidbody rb;
		
		private Vector3 playerVelocity;
	
		void Awake()
		{
			inputs = GetComponent<InputManager>();
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			Vector3 direction = transform.right * inputs.moveInput.x + transform.forward * inputs.moveInput.y;
			direction.Normalize();
			rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);
		}
	}
}
