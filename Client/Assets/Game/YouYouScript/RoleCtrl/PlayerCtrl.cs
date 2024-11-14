using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class PlayerCtrl : MonoBehaviour
{
    private YouYouJoystick mJoystick;
    private Camera mCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumpUp;
    private bool isJumpDown;
    private CharacterController mCharCtrl;
    private LayerMask groundMask;
    private Animator mAnimator;

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
    private Vector3? lastPlayerPos;
    private Vector3? lastPlayerRotate;
    
    public void InitParams(object[] param)
    {
        mAnimator = param[0] as Animator;
        mJoystick = param[1] as YouYouJoystick;
        mCamera = param[2] as Camera;
        
        mJoystick.OnChanged = StartMove;
        mJoystick.OnDown = null;
        mJoystick.OnUp = StopMove;
    }

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        mCharCtrl = GetComponent<CharacterController>();
        groundMask = LayerMask.GetMask("Default");
    }

    void Update()
    {
        if(mAnimator == null) return;
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

        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     EnableNavMove = true;
        //     NavMoveTo(new Vector3(11.72f, 0.05211985f, -9.18f), true, true);
        // }

        CheckKeyBoardMove();
        RecordPlayerPos();
        RecordPlayerRotate();
    }

    private Vector2 deltaVelocity = Vector2.zero; // 用于 SmoothDamp
    public float smoothTime = 0.1f; // 控制平滑时间
    public void CheckKeyBoardMove()
    {
#if UNITY_EDITOR
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 targetDelta = new Vector2(x, y);
        MoveDelta = Vector2.SmoothDamp(MoveDelta, targetDelta, ref deltaVelocity, smoothTime);
        if (x != 0f || y != 0f)
        {
            StartMove(MoveDelta);
        }
        else
        {
            StopMove(MoveDelta);
        }
#endif
    }

    private void RecordPlayerPos()
    {
        if (!lastPlayerPos.HasValue)
        {
            lastPlayerPos = transform.position;
        }
        else
        {
            if (lastPlayerPos.Value != transform.position)
            {
                lastPlayerPos = transform.position;
                GameEntry.Data.SetPlayerPos(lastPlayerPos.Value);
            }
        }
    }
    
    private void RecordPlayerRotate()
    {
        if (!lastPlayerRotate.HasValue)
        {
            lastPlayerRotate = transform.rotation.eulerAngles;
        }
        else
        {
            if (lastPlayerRotate.Value != transform.rotation.eulerAngles)
            {
                lastPlayerRotate = transform.rotation.eulerAngles;
                GameEntry.Data.SetPlayerRotate(lastPlayerRotate.Value);
            }
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

    private void UpdatePos(object param)
    {
        transform.position = (Vector3)param;
        transform.rotation = Quaternion.identity;
        PlayerRoleData playerRoleData = GameEntry.Data.PlayerRoleData;
        if (playerRoleData.firstEntryLevel)
        {
            GameEntry.Data.SetPlayerBornPos((Vector3)param);
        }
        else
        {
            transform.position = new Vector3(playerRoleData.playerPos[0],playerRoleData.playerPos[1],playerRoleData.playerPos[2]);
        }
    }
    
    private void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdatePlayerPos, UpdatePos);
    }

    private void OnDisable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdatePlayerPos, UpdatePos);
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