using UnityEngine;

public class PlayerControler : MonoBehaviour
{
	private InputManager _inputs;
	
	[SerializeField] private float _speed; 
	
	private Rigidbody _rb;
	
    void Start()
    {
	    _inputs = GetComponent<InputManager>();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
	    Vector3 movement = new Vector3(_inputs.MovX, 0.0f, _inputs.MovY);
	    _rb.AddForce(movement * _speed);
    }
}
