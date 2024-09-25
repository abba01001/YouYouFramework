
using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;
using UnityEngine.EventSystems;
using YouYou;

public class PlayerCamera : MonoBehaviour
{
    #region 常量

    public const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    public const string ERROR_UN_BINDCAM = "ThirdPersonCam脚本没有绑定摄像机!";
    public const string ERROR_UN_PLAYER = "ThirdPersonCam脚本没有指定玩家";

    /// <summary>
    /// 摄像机的基础方向
    /// </summary>
    private Vector3 CamBaseAxis = Vector3.back;

    /// <summary>
    /// 摄像机和碰撞体的交点向摄像机的观察点移动的距离
    /// </summary>
    private float CollisionReturnDis = 0.5f;

    #endregion

    #region 变量

    /// <summary>
    /// 摄像机
    /// </summary>
    private Transform mCamera;

    /// <summary>
    /// 玩家transform
    /// </summary>
    private Transform mPlayer;

    /// <summary>
    /// 角色中心点偏移
    /// </summary>
    public Vector3 mPivotOffset = new Vector3(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// 水平瞄准速度
    /// </summary>
    public float mHorizontalAimingSpeed = 1000.0f;

    /// <summary>
    /// 垂直瞄准速度
    /// </summary>
    public float mVerticalAimingSpeed = 1000.0f;

    /// <summary>
    /// 最大的垂直角度
    /// </summary>
    public float mMaxVerticalAngle = 30.0f;

    /// <summary>
    /// 最小的垂直角度
    /// </summary>
    public float mMinVerticalAngle = -60.0f;

    /// <summary>
    /// 基础摄像机偏移的倍率的最大值
    /// </summary>
    public float mMaxDistance = 10f;

    /// <summary>
    /// 基础摄像机偏移的倍率的最小值
    /// </summary>
    public float mMinDistance = 1.5f;

    /// <summary>
    /// 镜头推进的速度
    /// </summary>
    public float mZoomSpeed = 0.5f;

    /// <summary>
    /// 水平旋转的角度
    /// </summary>
    private float mAngleH = 0.0f;

    /// <summary>
    /// 垂直旋转的角度
    /// </summary>
    private float mAngleV = -30.0f;

    /// <summary>
    /// 基础摄像机偏移的倍率
    /// </summary>
    private float mDistance = 0.0f;

    #endregion

    private YouYouJoystick joystick;
    private bool RotateIng;
    private bool ResetRotateIng;
    private Vector2 RotateDelta = Vector2.zero;
    #region 内置函数

    public float initialAngleH = 0f;
    public float initialAngleV = -30f;
    
    private float speedFactor = 4f; // 初始速度因子
    private float acceleration = 0.5f; // 加速因子

    public void InitParams(object[] param)
    {
        mPlayer = param[0] as Transform;
        joystick = param[1] as YouYouJoystick;
        
        EasyTouch.On_Pinch += joystick.OnWidgetPinch;
        joystick.OnChanged += OnJoystickCamDragDown;
        joystick.OnUp += OnJoystickCamDragUp;
        joystick.OnPinch += OnJoystickCamPinch;
        mAngleH = initialAngleH;
        mAngleV = initialAngleV;
    }
    
    void Awake()
    {
        mCamera = GetComponent<Camera>().transform;
        mDistance = (mMinDistance + mMaxDistance) * 0.5f;
    }
    
	void Update ()
    {
        if (RotateIng)
        {
            float deltaMultiplier = Time.deltaTime * 30;  // 标准化到 60 FPS
            mAngleH += Mathf.Clamp(RotateDelta.x / 120, -1.0f, 1.0f) * mHorizontalAimingSpeed * deltaMultiplier;
            mAngleV += Mathf.Clamp(RotateDelta.y / 120, -1.0f, 1.0f) * mVerticalAimingSpeed * deltaMultiplier;
        }

        if (ResetRotateIng)
        {
            speedFactor += acceleration * Time.deltaTime;
            mAngleH = Mathf.Lerp(mAngleH, initialAngleH, Time.deltaTime * speedFactor);
            mAngleV = Mathf.Lerp(mAngleV, initialAngleV, Time.deltaTime  * speedFactor);
            if (Mathf.Abs(mAngleH - initialAngleH) <= 0.2f && Mathf.Abs(mAngleV - initialAngleV) <= 0.2f)
            {
                mAngleH = initialAngleH;
                mAngleV = initialAngleV;
                ResetRotateIng = false;
                speedFactor = 4f;
            }
        }
    }

    void OnDestroy()
    {
        EasyTouch.On_Pinch -= joystick.OnWidgetPinch;
        joystick.OnChanged -= OnJoystickCamDragDown;
        joystick.OnUp -= OnJoystickCamDragUp;
        joystick.OnPinch -= OnJoystickCamPinch;
    }

    void LateUpdate()
    {
        if (mCamera == null)
        {
            Debug.LogError(ERROR_UN_BINDCAM);
            return;
        }

        if (mPlayer == null)
        {
            Debug.LogError(ERROR_UN_PLAYER);
            return;
        }

        mAngleV = Mathf.Clamp(mAngleV, mMinVerticalAngle, mMaxVerticalAngle);
        mDistance = Mathf.Clamp(mDistance, mMinDistance, mMaxDistance);

        Quaternion animRotation = Quaternion.Euler(-mAngleV, mAngleH, 0.0f);
        Quaternion camYRotation = Quaternion.Euler(0.0f, mAngleH, 0.0f);
        mCamera.rotation = animRotation;

        Vector3 lookatpos = mPlayer.position + camYRotation * mPivotOffset;
        Vector3 camdir = animRotation * CamBaseAxis;
        camdir.Normalize();
        mCamera.position = lookatpos + camdir * mDistance;

        // 计算碰撞后的摄像机点
        RaycastHit rayhit;
        bool hit = Physics.Raycast(lookatpos, camdir, out rayhit, mDistance);
        if (hit)
        {
            // 屏蔽角色碰撞
            bool charcol = rayhit.collider as CharacterController;
            if (!charcol)
            {
                mCamera.position = rayhit.point - camdir * CollisionReturnDis;

                // 距离修正在范围内(1, 避免摄像机穿插进入角色)
                float distance = Vector3.Distance(mCamera.position, lookatpos);
                distance = Mathf.Clamp(distance, mMinDistance, mMaxDistance);
                mCamera.position = lookatpos + camdir * distance;
            }
        }
    }

#endregion

    #region 回调函数

    private void OnJoystickCamDragDown(Vector2 delta)
    {
        if(ResetRotateIng) return;
        RotateIng = true;
        RotateDelta = delta;
    }

    public void OnJoystickCamDragUp(Vector2 delta)
    {
        return;
        RotateIng = false;
        ResetRotateIng = true;
        RotateDelta = new Vector2(initialAngleH, initialAngleV);
    }
    
    private void OnJoystickCamPinch(float delta)
    {
        mDistance -= delta * mZoomSpeed;
    }

    #endregion

    #region 函数



    #endregion
}
