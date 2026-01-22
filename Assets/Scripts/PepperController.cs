using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PepperController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    public float jumpForce = 10f;
    [SerializeField]
    [Range(0f, 1f)] private float upwardBias = 0.8f;
    private float landingDelay = 0.5f;
    private bool canLand = false;
    private bool isLayingDown = false;

    private Vector2 moveInput;
    private float rotateInput;
    private Rigidbody rb;
    private Animator animator;
    private bool isJumping = false;
    private bool isMidair = false;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    [SerializeField] private AudioSource fanNoiseSource;
    [SerializeField] private AudioSource stepsSoundSource;
    [SerializeField] private AudioSource OneShotSource;

    [SerializeField] private AudioClip pepperLanding;
    [SerializeField] private AudioClip pepperCrouch;
    [SerializeField] private AudioClip pepperLay;
    [SerializeField] private AudioClip pepperStand;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && !isJumping && !isLayingDown)
        {
            Debug.Log("Jump pressed");
            isJumping = true;
            animator.SetTrigger("Jump");
            OneShotSource.PlayOneShot(pepperCrouch);
            StartCoroutine(LandingDelay());
        }
    }

    public void OnLay(InputAction.CallbackContext context)
    {
        if (isJumping) return;

        if (context.performed)
        {
            if (isLayingDown) OneShotSource.PlayOneShot(pepperStand);
            else OneShotSource.PlayOneShot(pepperLay);

            animator.SetBool("IsLaying", !animator.GetBool("IsLaying"));
            isLayingDown = !isLayingDown;
        }
    }

    IEnumerator LandingDelay()
    {
        yield return new WaitForSeconds(landingDelay);
        canLand = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isMidair && canLand && collision.collider.CompareTag("Ground"))
        {
            OneShotSource.PlayOneShot(pepperLanding);
            isMidair = false;
            isJumping = false;
            canLand = false;
        }

    }

    public void Jump_event()
    {
        Vector3 jumpDirection = Vector3.Lerp(transform.forward, transform.up, upwardBias).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
        isMidair = true;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isMidair) return;
        if (isLayingDown) return;
        moveInput = context.ReadValue<Vector2>();
        if (!stepsSoundSource.isPlaying) { stepsSoundSource.Play(); }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (isMidair) return;
        if (isLayingDown) return;
        rotateInput = context.ReadValue<float>();
        if (!stepsSoundSource.isPlaying) { stepsSoundSource.Play(); }
    }


    public void OnReset()
    {
        transform.SetPositionAndRotation(startingPosition, startingRotation);

        Animator animator = GetComponent<Animator>();
        ResetAllAnimatorParameters(animator);
        animator.Play("IdleState", 0, 0f);

        isJumping = false;
        isMidair = false;
        canLand = false;
        isLayingDown = false;
    }

    public void ResetAllAnimatorParameters(Animator animator)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.name, parameter.defaultBool);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.name, parameter.defaultFloat);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.name, parameter.defaultInt);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.ResetTrigger(parameter.name);
                    break;
            }
        }
    }


    private void Update()
    {
        if (moveInput.magnitude == 0 &&  rotateInput == 0)
        {
            stepsSoundSource.Stop();
        }
    }

    private void FixedUpdate()
    {

        Vector3 move = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
        bool isMoving = move.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            move.Normalize();
            rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
        }

        float rotationAmount = rotateInput * rotationSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(rotationAmount) > 0.001f)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationAmount, 0f));
            if(!isMoving) animator.SetBool("IsRotating", true);
        }
        else
        {
            animator.SetBool("IsRotating", false);
        }

        float absForward = Mathf.Abs(moveInput.y);
        float absRight = Mathf.Abs(moveInput.x);

        bool movingForward = false;
        bool movingBackward = false;
        bool movingRight = false;
        bool movingLeft = false;


        if (isMoving)
        {
            animator.SetBool("IsRotating", false);
            if (absForward >= absRight)
            {
                // Forward/backward dominates
                if (moveInput.y > 0f) movingForward = true;
                else if (moveInput.y < 0f) movingBackward = true;
            }
            else
            {
                // Lateral dominates
                if (moveInput.x > 0f) movingRight = true;
                else if (moveInput.x < 0f) movingLeft = true;
            }
        }

        animator.SetBool("IsMovingForward", movingForward);
        animator.SetBool("IsMovingBackward", movingBackward);
        animator.SetBool("IsMovingRight", movingRight);
        animator.SetBool("IsMovingLeft", movingLeft);
    }
}
