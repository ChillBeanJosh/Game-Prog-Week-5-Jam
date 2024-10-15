using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraOrientation : MonoBehaviour
{
    [Header("Refrence")]

    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rb;

    public float rotationSpeed;

    public CameraStyle currentStyle;

    public Transform combatLookAt;


    public enum CameraStyle
    {
        Basic,
        Combat
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //rotate orientation.
        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;

        //rotate player using orientation.
        if (currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDirection != Vector3.zero)
            {
                playerObject.forward = Vector3.Slerp(playerObject.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
            }

        }
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 combatDirection = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = combatDirection.normalized;

            playerObject.forward = combatDirection.normalized;

        }

    }
}
