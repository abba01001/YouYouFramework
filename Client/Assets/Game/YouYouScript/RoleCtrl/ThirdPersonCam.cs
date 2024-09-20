
using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;
using UnityEngine;
using UnityEngine.EventSystems;
using YouYou;

public class ThirdPersonCam : MonoBehaviour
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
    public Transform mPlayer;

    /// <summary>
    /// 角色中心点偏移
    /// </summary>
    public Vector3 mPivotOffset = new Vector3(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// 水平瞄准速度
    /// </summary>
    public float mHorizontalAimingSpeed = 1000.0f;
    
#if UNITY_EDITOR
    public float scaleFactor = 1f;
#else
    public float scaleFactor = 10f;
#endif

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
    public float mMaxDistance = 2.0f;

    /// <summary>
    /// 基础摄像机偏移的倍率的最小值
    /// </summary>
    public float mMinDistance = 1.0f;

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

    /// <summary>
    /// 控制摄像机的ui
    /// </summary>
    public JoystickCamUI mJoystickCamUI;

    #endregion

    public YouYouJoystick Joystick;
    private bool RotateIng;
    private bool ResetRotateIng;
    private Vector2 RotateDelta = Vector2.zero;
    #region 内置函数

    public float initialAngleH = 0f;
    public float initialAngleV = -30f;
    
    private float speedFactor = 4f; // 初始速度因子
    private float acceleration = 0.5f; // 加速因子
    void Awake()
    {
        mCamera = GetComponent<Camera>().transform;
        mDistance = (mMinDistance + mMaxDistance) * 0.5f;
    }

    // Use this for initialization
	void Start ()
    {
        EasyTouch.On_Pinch += Joystick.OnWidgetPinch;
        Joystick.OnChanged += OnJoystickCamDragDown;
        Joystick.OnUp += OnJoystickCamDragUp;
        Joystick.OnPinch += OnJoystickCamPinch;
        mAngleH = initialAngleH;
        mAngleV = initialAngleV;
        // mJoystickCamUI.OnDrag += OnJoystickCamDragDown;
        // mJoystickCamUI.OnPinch += OnJoystickCamPinch;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (RotateIng)
        {
            mAngleH += Mathf.Clamp(RotateDelta.x / Screen.width, -1.0f, 1.0f) * mHorizontalAimingSpeed * scaleFactor;
            mAngleV += Mathf.Clamp(RotateDelta.y / Screen.height, -1.0f, 1.0f) * mVerticalAimingSpeed * scaleFactor;
        }

        if (ResetRotateIng)
        {
            speedFactor += acceleration * Time.deltaTime;
            mAngleH = Mathf.Lerp(mAngleH, initialAngleH, Time.deltaTime * speedFactor * scaleFactor);
            mAngleV = Mathf.Lerp(mAngleV, initialAngleV, Time.deltaTime  * speedFactor * scaleFactor);
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
        if (mJoystickCamUI != null)
        {
            mJoystickCamUI.OnDrag -= OnJoystickCamDragDown;
            mJoystickCamUI.OnPinch -= OnJoystickCamPinch;
        }
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
