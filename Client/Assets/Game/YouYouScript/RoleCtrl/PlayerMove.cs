﻿using UnityEngine;
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
    private Vector2 MoveDelta = Vector2.zero;

    void Start()
    {
        mJoystick.OnChanged = StartMove;
        mJoystick.OnDown = null;
        mJoystick.OnUp = StopMove;

        groundMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        CheckJump();
        CheckMove();
        mCharCtrl.Move(velocity * Time.deltaTime);
    }

    private void CheckJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, Constants.GroundCheckDistance, groundMask);
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            mAnimator.SetBool("isGround", false);
            mAnimator.SetTrigger("jump");
            velocity.y = Mathf.Sqrt(Constants.MainRoleJumpHeight * -2f * Constants.GRAVITY);
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