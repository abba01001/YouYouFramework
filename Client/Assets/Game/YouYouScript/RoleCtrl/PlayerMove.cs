using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public JoystickUI mJoystick;

    public Camera mCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumpUp;
    private bool isJumpDown;
    private float jumpHeight = 1f;
    private float gravity = -9.81f;
    private float groundCheckDistance = 0.1f; // 射线检测长度
    private float moveSpeed = 1.5f;
    public CharacterController mCharCtrl;
    private LayerMask groundMask;
    [SerializeField]private Animator mAnimator;

#if PLAYMOVEUSETEMPTRANS
    private Transform mTemp;
#endif

    #region 内置函数

    // Use this for initialization
	void Start () 
    {
        mJoystick.OnMove = OnJoystickMove;
        mJoystick.OnMoveEnd = OnJoystickMoveEnd;
        groundMask = LayerMask.GetMask("Ground");
#if PLAYMOVEUSETEMPTRANS
        GameObject go = new GameObject("TempJoystick");
        mTemp = go.transform;
#endif

    }

    // Update is called once per frame
    void Update () 
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
        // 处理跳跃输入
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            mAnimator.SetBool("isGround",false);
            Jump();
        }

        // 应用重力
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            if (velocity.y < 0) // 确保在地面时重置速度
            {
                velocity.y = 0f;
                mAnimator.SetBool("isGround",true);
            }
        }
        Debug.LogError(velocity.y);

        // 施加最终速度
        mCharCtrl.Move(velocity * Time.deltaTime);
    }
    
    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        mAnimator.SetTrigger("jump"); // 假设有一个跳跃动画
    }

#endregion

    private void OnJoystickMove(Vector2 delta)
    {
        Vector3 realdir = new Vector3(delta.x, 0.0f, delta.y);

#if PLAYMOVEUSETEMPTRANS
        mTemp.eulerAngles = new Vector3(0.0f, mCamera.transform.eulerAngles.y, 0.0f);
        realdir = mTemp.TransformDirection(realdir);
#else
        realdir = Quaternion.AngleAxis(mCamera.transform.eulerAngles.y, Vector3.up) * realdir;
#endif

        float angle = Vector3.Angle(transform.forward, realdir);
        realdir = Vector3.Slerp(transform.forward, realdir, Mathf.Clamp01(180 * Time.deltaTime * 5 / angle));
        transform.LookAt(transform.position + realdir);

        // 使用 velocity 来计算水平移动
        velocity.x = realdir.x * moveSpeed; // 设置水平速度
        velocity.z = realdir.z * moveSpeed; // 设置水平速度

        mCharCtrl.Move(velocity * Time.deltaTime);//realdir * 5);
        mAnimator.SetBool("run", true);
    }

    private void OnJoystickMoveEnd()
    {
        mAnimator.SetBool("run", false);
    }
}
