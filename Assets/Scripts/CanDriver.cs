using UnityEngine;
using UnityEngine.InputSystem;

public class CanDriver : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 move = (transform.forward * moveInput.y) + (transform.right * moveInput.x);

        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
}