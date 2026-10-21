using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool faceMoveDirection = true;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float fallVelocityThreshold = 0.1f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isRunning;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int IsRunningHash =
        Animator.StringToHash("IsRunning");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int IsFallingHash =
        Animator.StringToHash("IsFalling");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator == null)
            Debug.LogError("Animator not found!", this);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
            isRunning = true;

        if (context.canceled)
            isRunning = false;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            if (animator != null)
                animator.SetTrigger(JumpHash);
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        MovePlayer();
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        bool isFalling =
            !isGrounded &&
            rb.linearVelocity.y < -fallVelocityThreshold;

        animator.SetBool(IsMovingHash, isMoving);

        // Run animation only when moving and holding Shift.
        animator.SetBool(
            IsRunningHash,
            isRunning && isMoving && isGrounded
        );

        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsFallingHash, isFalling);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void MovePlayer()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement =
            forward * moveInput.y +
            right * moveInput.x;

        float currentSpeed =
            isRunning && moveInput.sqrMagnitude > 0.01f
                ? runSpeed
                : moveSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * currentSpeed;
        velocity.z = movement.z * currentSpeed;

        rb.linearVelocity = velocity;

        if (faceMoveDirection && movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(movement, Vector3.up);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    10f * Time.fixedDeltaTime
                )
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}