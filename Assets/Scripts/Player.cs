﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [Header("Basics Settings")]
    [Range(0, 150)] public int hp = 100;
    [Range(0.05f, 0.5f)] float turnSmoohtTime = 0.1f;
    [Range(1, 4)] public int walkSpeed = 3;
    [Range(4, 6)] public int runSpeed = 5;
    [Range(4, 6)] public float jumpSpeed = 4;
    [Range(6, 10)] public float gravity = 8;

    [Header("Physics Settings")]
    public Raycaster groundRaycaster;
    public Raycaster wallInFrontRaycaster;

    int currentSpeed;
    float turnSmoohtVelocity;
    Transform mainCamera;
    Animator animator;
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
        get {return !groundRaycaster.Check();}
    }
    
    void Start()
    {
        mainCamera = Camera.main.transform;
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        animator.SetBool("isRunning", IsRunning);
        animator.SetBool("isWalking", IsWalking);
        animator.SetBool("isFalling", IsFalling);
    }

    void Move()
    {
        if (!IsAlive)
            return;

        inputsVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
            
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

    void Jump()
    {
        if (wallInFrontRaycaster.Check())
        {
            animator.SetTrigger("jumpToBraced");
        } else {
            animator.SetTrigger("jump");
        }
    }
}