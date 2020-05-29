using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Range(0, 150)] public int hp = 100;
    [Range(1, 4)] public int walkSpeed = 3;
    [Range(4, 6)] public int runSpeed = 5;
    [Range(4, 6)] public float jumpSpeed = 4;
    [Range(6, 10)] public float gravity = 8;

    int currentSpeed;
    Animator animator;
    CharacterController controller;
    Vector3 inputsVector;
    Vector3 moveDirection;
    
    public bool IsAlive
    {
        get {return hp > 0;}
    }

    public bool IsWalking
    {
        get { return moveDirection.magnitude > 0 && currentSpeed == walkSpeed; }
    }

    public bool IsRunning
    {
        get { return moveDirection.magnitude > 0 && currentSpeed == runSpeed; }
    }

    public bool IsFalling
    {
        get {return !controller.isGrounded;}
    }
    
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsAlive)
            return;

        if (controller.isGrounded)
        {
            inputsVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = Camera.main.transform.TransformDirection(inputsVector);

            moveDirection.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                //moveDirection.y = jumpSpeed;
                animator.SetTrigger("jumpToBraced");
            }
                
            // Horizontal Axis Moved or Vertical Axis Moved or 
            if (Mathf.Abs(inputsVector.x) != 0 || Mathf.Abs(inputsVector.z) != 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z)), Time.deltaTime * 10);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentSpeed = runSpeed;
                }
                else
                {
                    currentSpeed = walkSpeed;
                }
            } else {
                currentSpeed = 0;
            }

            moveDirection = new Vector3(moveDirection.x * currentSpeed, moveDirection.y, moveDirection.z * currentSpeed);
        }

        moveDirection.y -= gravity * Time.deltaTime;
        
        controller.Move(moveDirection * Time.deltaTime);
    }

    void FixedUpdate()
    {
        animator.SetBool("isRunning", IsRunning);
        animator.SetBool("isWalking", IsWalking);
        animator.SetBool("isFalling", IsFalling);
    }
}