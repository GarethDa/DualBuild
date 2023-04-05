using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedAnimation : MonoBehaviour
{
    Animator animator;
    bool onDaFloor = false;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] GameObject feet;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        onDaFloor = Physics.CheckSphere(feet.transform.position, 0.1f, floorMask);

        if (onDaFloor == false)
        {
            animator.Play("Fall");
        }
        else if (GetComponent<NetworkedVelocity>().newVelocity.magnitude > 1)
        {
            animator.Play("Run");
        }
        else
        {
            animator.Play("idle");
        }
    }
}
