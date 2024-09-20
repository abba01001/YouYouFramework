﻿using DunGen.DungeonCrawler;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class PlayerMove : MonoBehaviour
{
    public YouYouJoystick mJoystick;


    public Camera mCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumpUp;
    private bool isJumpDown;
    public CharacterController mCharCtrl;
    private LayerMask groundMask;
    [SerializeField] private Animator mAnimator;

    private bool IsMove = false;
    private bool IsJump = false;
    private Vector2 MoveDelta = Vector2.zero;


    public bool EnableOperateMove = false;
    public bool EnableNavMove = false;
    
    

    public NavMeshAgent Agent { get; private set; }
    
    [SerializeField]
    private GameObject pingPrefab = null;
    private Vector3 manualMovementDirection;
    private NavMeshPath path;
    
    
    private float verticalVelocity = 0f;      // 垂直速度
    private float slopeForce = 2f;            // 控制下坡时的力度
    private float slopeForceRayLength = 1.5f; // 射线检测的长度
    
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        mJoystick.OnChanged = StartMove;
        mJoystick.OnDown = null;
        mJoystick.OnUp = StopMove;

        groundMask = LayerMask.GetMask("Default");
    }

    void Update()
    {
        if (EnableOperateMove)
        {
            CheckOperateMove();
        }

        if (EnableNavMove)
        {
            if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                mAnimator.SetBool("run", false);
                EnableNavMove = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            EnableNavMove = true;
            NavMoveTo(new Vector3(11.72f, 0.05211985f, -9.18f), true, true);
        }

    }

    public void NavMoveTo(Vector3 destination,bool forceCalculatePath,bool pingLocation)
    {
        mAnimator.SetBool("isGround", true);
        mAnimator.SetBool("run", true);
        NavMeshHit hit;
        Agent.enabled = true;
        Agent.speed = Constants.MainRoleMoveSpeed;
        if (NavMesh.SamplePosition(destination, out hit, 2f, -1)) destination = hit.position;
            
        bool requiresPath;

        if (forceCalculatePath)
            requiresPath = true;
        else
        {
            const float manualMoveDistanceThreshold = 1.5f;
            float distanceToDestination = (transform.position - destination).magnitude;

            if (distanceToDestination > manualMoveDistanceThreshold)
                requiresPath = true;
            else
                requiresPath = NavMesh.Raycast(transform.position, destination, out hit, -1);
        }

        if (!requiresPath)
        {
            manualMovementDirection = (destination - transform.position);
            manualMovementDirection.y = 0f;
            manualMovementDirection.Normalize();
        }
        else
        {
            manualMovementDirection = Vector3.zero;
            if (Agent.CalculatePath(destination, path))
            {
                Agent.path = path;
            }
        }
        
        if (pingLocation)
            Instantiate(pingPrefab, destination, Quaternion.identity);
    }

    private void CheckOperateMove()
    {
        CheckJump();
        CheckMove();
        CollisionFlags flags = mCharCtrl.Move(velocity * Time.deltaTime);
        HandleInWall(flags);

    }

    // 让角色滑过碰撞的表面，避免卡住
    private void HandleInWall(CollisionFlags flags)
    {
        if ((flags & CollisionFlags.Sides) != 0)
        {
            velocity += Physics.gravity * Time.deltaTime; // 遇到侧面碰撞时增加重力的影响
        }
    }

    public void OperateJump()
    {
        if(!isGrounded) return;
        IsJump = true;
    }
    
    private void CheckJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, Constants.GroundCheckDistance, groundMask);
        if (isGrounded && IsJump)
        {
            mAnimator.SetBool("isGround", false);
            mAnimator.SetTrigger("jump");
            velocity.y = Mathf.Sqrt(Constants.MainRoleJumpHeight * -2f * Constants.GRAVITY);
            IsJump = false;
        }

        if (!isGrounded)
        {
            velocity.y += Constants.GRAVITY * Time.deltaTime;
        }
        else
        {
            if (velocity.y < 0) // 确保在地面时重置速度
            {
                velocity.y = 0f;
                mAnimator.SetBool("isGround", true);
            }
        }
    }

    private void CheckMove()
    {
        if (IsMove)
        {
            Vector3 realdir = new Vector3(MoveDelta.x, 0.0f, MoveDelta.y);
            realdir = Quaternion.AngleAxis(mCamera.transform.eulerAngles.y, Vector3.up) * realdir;

            float angle = Vector3.Angle(transform.forward, realdir);
            realdir = Vector3.Slerp(transform.forward, realdir, Mathf.Clamp01(180 * Time.deltaTime * 5 / angle));
            transform.LookAt(transform.position + realdir);
            velocity.x = realdir.x * Constants.MainRoleMoveSpeed; // 设置水平速度
            velocity.z = realdir.z * Constants.MainRoleMoveSpeed; // 设置水平速度
            mAnimator.SetBool("run", true);
        }
        if (isGrounded)
        {
            verticalVelocity = 0f; // 如果角色在地面上，垂直速度重置

            // 检查下坡并添加额外的力
            if (OnSlope())
            {
                velocity += Vector3.down * mCharCtrl.height / 2 * slopeForce;
                velocity.y = verticalVelocity;
            }
        }else
        {
            verticalVelocity += Constants.GRAVITY * Time.deltaTime; // 角色不在地面时，应用重力
        }
    }
    
    private bool OnSlope()
    {
        if (IsMove) // 只有在移动时才检查
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                return angle > mCharCtrl.slopeLimit; // 返回是否超过斜坡限制的角度
            }
        }
        return false;
    }

    private void StartMove(Vector2 delta)
    {
        IsMove = true;
        MoveDelta = delta;
    }

    private void StopMove(Vector2 delta)
    {
        IsMove = false;
        velocity = Vector3.zero;
        mAnimator.SetBool("run", false);
    }
}