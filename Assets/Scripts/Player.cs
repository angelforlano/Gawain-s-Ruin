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
    
    [Header("Foots Ik Settings")]
    public float pelvisOffset = 1;
    [Range(0f, 2f)] public float raycastDonwDistance = 1.5f;
    public float heigtFromGroundRaycast = 1;
    public float feetToIkPositionSpeed = 2;
    public float pelvisUpAndDonwSpeed = 2;

    [Header("Physics Settings")]
    public LayerMask environmentLayerMask;
    public Raycaster groundRaycaster;
    public Raycaster wallInFrontRaycaster;
    public Raycaster wallRightRaycaster;
    public Raycaster wallLeftRaycaster;

    [Header("Other Settings")]
    public Transform playerMesh;
    
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

    float lastPelvisPositionY;
    float lastRightFootPositionY;
    float lastlLeftFootPositionY;
    Vector3 rightFootPosition;
    Vector3 leftFootPosition;
    Vector3 rightFootIKPosition;
    Vector3 leftFootIKPosition;
    Quaternion rightFootIKRotation;
    Quaternion leftFootIKRotation;
    
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

    void AlignToTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position ;
        Quaternion rotation = Quaternion.FromToRotation(transform.forward, direction);
        transform.rotation = rotation * transform.rotation;        
    }

    void Start()
    {
        mainCamera = Camera.main.transform;
        animator = gameObject.GetComponentInChildren<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdateInputs();
        UpdateMovement();
        UpdateClimbMovement();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);
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
            transform.Translate(new Vector3(horizontalMovement, 0, 0) * (walkSpeed/2) * Time.deltaTime);
        } else {
            animator.SetFloat("hMovement", 0);
        }   
    }

    void Jump()
    {
        if(IsFalling) return;
        
        if (wallInFrontRaycaster.Check())
        {
            StartClimbing();
        } else {
            animator.SetTrigger("jump");
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void StartClimbing()
    {
        if (!climbing)
        {
            climbing = true;
            animator.SetTrigger("jumpToBraced");
            
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, (wallInFrontRaycaster.GetHitNormal()* -1));
            
            playerMesh.localPosition = new Vector3(0, 0, -0.4f);
            playerMesh.localRotation = Quaternion.Euler(10, 0, 0);
        } else {
           animator.SetTrigger("climbing");
        }
    }

    public void StopClimbing()
    {
        climbing = false;
    }

    #region FootIKSystem
    void MovePelvisHeigt()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDonwSpeed);

        animator.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = animator.bodyPosition.y;
    }

    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = animator.GetIKPosition(foot);

        if(positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);

            animator.SetIKRotation(foot, rotationIkHolder);
        }

        animator.SetIKPosition(foot, targetIkPosition);
    }

    void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        RaycastHit feetOutHit;

        // We cast our ray from above the foot in case the current terrain/floor is above the foot position.
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDonwDistance + heigtFromGroundRaycast, environmentLayerMask))
        {
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }

        feetIkPositions = Vector3.zero;
    }

    void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heigtFromGroundRaycast;
    }

    void OnAnimatorIK(int layerIndex)
    {
        MovePelvisHeigt();

        // Set the weights of left and right feet to the current value defined by the curve in our animatorations.
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight") );
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight") );

        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastlLeftFootPositionY);
    }
    #endregion
}