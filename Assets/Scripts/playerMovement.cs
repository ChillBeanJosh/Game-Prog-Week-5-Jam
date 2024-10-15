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
    public float airGravityMultiplier = 0.5f; 
    bool jumpReady;

    [Header("Drag Settings")]
    public float minAirDrag = 0.1f; 
    public float maxAirDrag = 5.0f; 

    [Header("Keybinds")]
    public KeyCode jumpKey;

    [Header("Ground Checks")]
    public float playerHeight;
    public LayerMask Ground;
    bool isgrounded;

    public Transform orientation;
    public Transform playerObject;  
    public Transform cameraTransform;  

    [Header("Raycast")]
    public float rayDistance = 1.5f;

    public Animator animator;

    private Rigidbody rb;

    float horizontalInput;
    float verticalInput;
    Vector3 movementDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        //rb null check
        if (rb == null)
        {
            Debug.LogError("Rigidbody is not getting called"); 
        }

        rb.freezeRotation = true;
        jumpReady = true;
    }

    private void Update()
    {
        GroundCheck(); 

        //ground drag and resets rotation while grounded
        if (isgrounded)
        {
            rb.drag = groundDrag;  
            ResetAirRotation();    
        }
        //air drag and adjusts rotation
        else
        {
            rb.drag = CalculateAirDrag();  
            RotatePlayerInAir();         
        }

        PlayerInputs();   
        SpeedController(); 
    }

    private void FixedUpdate()
    {
        playerMove();  

        //custom gravity when not grounded
        if (!isgrounded)
        {
            AdjustGravity();  
        }
    }

    private void PlayerInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

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
        movementDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //adds force for grounded movement
        if (isgrounded)
        {
            rb.AddForce(movementDirection.normalized * movementSpeed * 10f, ForceMode.Force);

            if (movementDirection.magnitude > 0)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isAirWalking", false); 
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isAirWalking", false); 
            }
        }
        //ads force for air movement
        else
        {
            rb.AddForce(movementDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);

            if (movementDirection.magnitude > 0)
            {
                animator.SetBool("isAirWalking", true); 
                animator.SetBool("isWalking", false); 
            }
            else
            {
                animator.SetBool("isAirWalking", false); 
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

    //function for jump force
    void JumpAction()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    void JumpReset()
    {
        jumpReady = true;
    }

    //function for raycast to detect ground layer
    void GroundCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.3f, Ground))
        {
            isgrounded = true;   
            transform.SetParent(hit.transform); 
        }
        else
        {
            isgrounded = false; 
            transform.SetParent(null); 
        }
    }

    //active whenever player is not grounded
    void RotatePlayerInAir()
    {
        //rotates 90 on the x
        playerObject.localRotation = Quaternion.Euler(90f, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);

        //gets camera angle for x(up)
        float cameraAngle = Vector3.Angle(cameraTransform.forward, Vector3.up); 

        //gets camera angle - 90 so have the player rotate with cam, - 90f to keep sideways
        float rotationAmount = cameraAngle - 90f; 

        //sets limits on the rotation
        rotationAmount = Mathf.Clamp(rotationAmount, -90f, 90f);

        //apply rotation to player object with the calculations
        playerObject.localRotation = Quaternion.Euler(90f + rotationAmount, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);
    }

    //active whenever player is grounded
    void ResetAirRotation()
    {
        //resets player x axis to 0
        playerObject.localRotation = Quaternion.Euler(0f, playerObject.localRotation.eulerAngles.y, playerObject.localRotation.eulerAngles.z);
    }

    // Dynamic air drag calculation based on camera and player orientation
    float CalculateAirDrag()
    {
        //gets a vector of the camera wherever you are looking at
        Vector3 cameraForward = cameraTransform.forward;

        //gets the angle between the vector(camera direction) and vector(up)
        //dot product of the 2 vectors divided by the magnitude of the 2 vectors multiplied = cos(angle).
        float angle = Vector3.Angle(cameraForward, Vector3.up);

        //angle range from 0 to 180
        float normalizedAngle = Mathf.InverseLerp(0f, 180f, angle);

        //using max and min and a range [correspondes with 0 to 180 on the normalized Angle to get the drag]
        float dragFactor = Mathf.Lerp(maxAirDrag, minAirDrag, normalizedAngle);

        return dragFactor;
    }

    //function for custom air gravity
    void AdjustGravity()
    {
        Vector3 velocity = rb.velocity;
        //custom gravity
        velocity.y += Physics.gravity.y * airGravityMultiplier * Time.fixedDeltaTime;
        rb.velocity = velocity;
    }

}
