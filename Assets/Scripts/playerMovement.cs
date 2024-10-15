using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;
    public float groundDrag;

    public float jumpPower;
    public float jumpCooldown;
    public float airMultiplier;
    public float airGravityMultiplier = 0.5f; // Adjust the gravity when airborne
    bool jumpReady;

    [Header("Drag Settings")]
    public float minAirDrag = 0.1f; // Minimum drag applied in the air
    public float maxAirDrag = 5.0f; // Maximum drag applied in the air

    [Header("Keybinds")]
    public KeyCode jumpKey;

    [Header("Ground Checks")]
    public float playerHeight;
    public LayerMask Ground;
    bool isgrounded;

    public Transform orientation;
    public Transform playerObject;  // The child object representing the player's visual model
    public Transform cameraTransform;  // Reference to the camera's transform

    [Header("Raycast")]
    public float rayDistance = 1.5f;  // Distance to check if grounded

    public Animator animator;

    private Rigidbody rb;

    // Input and movement variables
    float horizontalInput;
    float verticalInput;

    Vector3 movementDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody is not getting called"); // null check
        }

        rb.freezeRotation = true;
        jumpReady = true;
    }

    private void Update()
    {
        GroundCheck();  // Perform ground check

        // Handle drag control depending on whether the player is grounded or not
        if (isgrounded)
        {
            rb.drag = groundDrag;  // Set normal ground drag
            ResetAirRotation();    // Reset air rotation when grounded
        }
        else
        {
            rb.drag = CalculateAirDrag();  // Apply dynamic air drag based on orientation
            RotatePlayerInAir();           // Apply air rotation while airborne (falling or moving)
        }

        PlayerInputs();   // Handle player input
        SpeedController();  // Handle speed
    }

    private void FixedUpdate()
    {
        playerMove();  // Handle movement in FixedUpdate

        // Adjust vertical velocity if airborne to apply gravity effect
        if (!isgrounded)
        {
            AdjustGravity();  // Apply custom gravity scale
        }
    }

    private void PlayerInputs()
    {
        // Get player input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump logic
        if (Input.GetKey(jumpKey) && jumpReady && isgrounded)
        {
            animator.SetTrigger("Jump");
            jumpReady = false;
            JumpAction();
            Invoke(nameof(JumpReset), jumpCooldown);
        }
    }

    void playerMove()
    {
        // Calculate movement direction
        movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isgrounded)
        {
            // Grounded movement
            rb.AddForce(movementDirection.normalized * movementSpeed * 10f, ForceMode.Force);

            // Update animator states for grounded movement
            if (movementDirection.magnitude > 0)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isAirWalking", false); // Reset air walking
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isAirWalking", false); // Reset air walking
            }
        }
        else
        {
            // Airborne movement (apply air multiplier)
            rb.AddForce(movementDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);

            // Update animator states for airborne movement
            if (movementDirection.magnitude > 0)
            {
                animator.SetBool("isAirWalking", true); // Set air walking
                animator.SetBool("isWalking", false); // Reset walking
            }
            else
            {
                animator.SetBool("isAirWalking", false); // Reset air walking if no input
            }
        }
    }

    void SpeedController()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude > movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    void JumpAction()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    void JumpReset()
    {
        jumpReady = true;
    }

    // Ground check to update isgrounded
    void GroundCheck()
    {
        RaycastHit hit;

        // Raycast to check if grounded
        if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.3f, Ground))
        {
            isgrounded = true;   // Player is grounded
            transform.SetParent(hit.transform);  // Set parent when grounded
        }
        else
        {
            isgrounded = false;  // Player is in the air
            transform.SetParent(null);  // Unparent when in the air
        }
    }

    // Handle player rotation in the air
    void RotatePlayerInAir()
    {
        // Set the player object to 90 degrees on the X-axis initially
        playerObject.localRotation = Quaternion.Euler(90f, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);

        // Calculate the angle based on the camera's forward vector
        float cameraAngle = Vector3.Angle(cameraTransform.forward, Vector3.up);  // Get angle from up vector

        // Map the angle to adjust the player's rotation
        float rotationAmount = cameraAngle - 90f; // Calculate rotation based on camera angle
        rotationAmount = Mathf.Clamp(rotationAmount, -90f, 90f); // Clamp to prevent excessive flipping

        // Apply rotation to the player object based on the camera's angle
        playerObject.localRotation = Quaternion.Euler(90f + rotationAmount, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);
    }

    // Reset player rotation when grounded
    void ResetAirRotation()
    {
        // Reset rotation to 0 degrees on the X-axis when grounded
        playerObject.localRotation = Quaternion.Euler(0f, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);
    }

    // Dynamic air drag calculation based on camera and player orientation
    float CalculateAirDrag()
    {
        // Get the forward vector of the camera
        Vector3 cameraForward = cameraTransform.forward;

        // Calculate the angle between the camera's forward vector and the up direction
        float angle = Vector3.Angle(cameraForward, Vector3.up); // Angle from the up direction

        // Normalize the angle to a range from 0 (up) to 180 (down)
        float normalizedAngle = Mathf.InverseLerp(0f, 180f, angle); // Map angle to [0, 1]

        // Use the normalized angle to interpolate between maxAirDrag and minAirDrag
        float dragFactor = Mathf.Lerp(maxAirDrag, minAirDrag, normalizedAngle);

        return dragFactor;
    }

    // Adjust vertical velocity to apply custom gravity while airborne
    void AdjustGravity()
    {
        // Modify the vertical velocity to apply custom gravity while in the air
        Vector3 velocity = rb.velocity;
        velocity.y += Physics.gravity.y * airGravityMultiplier * Time.fixedDeltaTime; // Apply custom gravity scale
        rb.velocity = velocity; // Update Rigidbody velocity
    }

}
