using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [Header("Basics Settings")]
    [Range(0, 150)] public int hp = 100;
    [Range(0.05f, 0.5f)] float turnSmoohtTime = 0.1f;
    [Range(1, 4)] public int walkSpeed = 3;
    [Range(4, 6)] public int runSpeed = 5;
    [Range(4, 6)] public float jumpForce = 4;
    [Range(6, 10)] public float gravity = 8;

    [Header("Physics Settings")]
    public Raycaster groundRaycaster;
    public Raycaster wallInFrontRaycaster;
    public Raycaster wallRightRaycaster;
    public Raycaster wallLeftRaycaster;

    int currentSpeed;
    float turnSmoohtVelocity;
    Transform mainCamera;
    Animator animator;
    Rigidbody rb;

    bool climbing;
    float horizontalMovement;
    float verticalMovement;
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

    public bool IsGrounded
    {
        get {return groundRaycaster.Check();}
    }

    public bool IsFalling
    {
        get {return !IsGrounded;}
    }

    void Start()
    {
        mainCamera = Camera.main.transform;
        animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdateInputs();
        UpdateMovement();
        UpdateClimbMovement();
        UpdateAnimator();
    }

    void UpdateInputs()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        inputsVector = new Vector3(horizontalMovement, 0, verticalMovement).normalized;
        
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    void UpdateAnimator()
    {
        animator.SetBool("isRunning", IsRunning);
        animator.SetBool("isWalking", IsWalking);
        animator.SetBool("isFalling", IsFalling);
    }

    void UpdateMovement()
    {
        if (!IsAlive || climbing) return;
    
        if (inputsVector.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(inputsVector.x, inputsVector.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoohtVelocity, turnSmoohtTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (wallInFrontRaycaster.Check())
            {
                currentSpeed = 0;
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                currentSpeed = runSpeed;
            } else {
                currentSpeed = walkSpeed;
            }

            if (currentSpeed > 0 )
            {
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                transform.Translate(moveDirection.normalized * currentSpeed * Time.deltaTime, Space.World);
            }
        } else {
            currentSpeed = 0;
        }     
    }

    void UpdateClimbMovement()
    {
        if(!climbing) return;

        if((wallLeftRaycaster.Check() && horizontalMovement < 0) || (wallRightRaycaster.Check() && horizontalMovement > 0))
        {
            animator.SetFloat("hMovement", horizontalMovement);
            transform.Translate(new Vector3(horizontalMovement, 0, 0) * (walkSpeed/2) * Time.deltaTime, Space.World);
        } else {
            animator.SetFloat("hMovement", 0);
        }   
    }

    void Jump()
    {
        if(IsFalling) return;
        
        if (wallInFrontRaycaster.Check())
        {
            animator.SetTrigger("jumpToBraced");
            climbing = true;
        } else {
            animator.SetTrigger("jump");
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }
}