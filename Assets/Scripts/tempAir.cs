using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempAir : MonoBehaviour
{
    [Header("Refrences")]
    public Transform orientation;
    //public Transform playerCam;
    private Rigidbody rb;
    private playerMovement pm;
    public Animator animator;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float leapCd;
    private float leapCdTimer;

    [Header("Input")]
    public KeyCode leapKey;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<playerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(leapKey))
        {
            Leap();
        }

        if (leapCdTimer > 0)
        {
            leapCdTimer -= Time.deltaTime;

        }

    }
    private void Leap()
    {

        if (leapCdTimer > 0) return;
        else leapCdTimer = leapCd;

        animator.SetTrigger("Leap");



        Vector3 forceToApply = orientation.up * dashForce + orientation.up * dashUpwardForce;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        Invoke(nameof(ResetLeap), dashDuration);
    }

    private void ResetLeap()
    {

    }
}
