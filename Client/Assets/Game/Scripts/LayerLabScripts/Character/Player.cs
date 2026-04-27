using System;
using OctoberStudio.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LayerLab.ArtMaker
{
    /// <summary>
    /// 플레이어 캐릭터 컨트롤러
    /// Player character controller
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// 플레이어 상태 열거형
        /// Player state enumeration
        /// </summary>
        public enum EPlayerState
        {
            Idle,
            Run
        }

        public EPlayerState PlayerState { get; set; } // 현재 플레이어 상태 / Current player state

        public static Player Instance { get; private set; }
        [field: SerializeField] public PartsManager PartsManager { get; private set; } // 부품 매니저 참조 / Parts manager reference

        [SerializeField] private float moveSpeed = 5f; // 이동 속도 / Move speed
        [SerializeField] private float stoppingDistance = 0.1f; // 정지 거리 / Stopping distance
        [SerializeField] private float rotationSpeed = 10f; // 회전 속도 / Rotation speed

        private bool InDistance => InputDistance() <= stoppingDistance;
        [SpineEvent] private readonly string _footStepEventName = "Step"; // 발소리 이벤트 이름 / Footstep event name
        private Vector3 _targetPosition;
        private Vector3 _firstPosition;
        private Vector2 _moveDirection;
        private Rigidbody2D _rigidbody;
        private bool _isMoving;
        private bool _isDragging;
        private bool _canMove;

        private Collider2D _characterCollider;

        [SerializeField] private JoystickBehavior joystick;
        [SerializeField] private bool autoInit; // 자동 초기화 활성화 여부 / Whether auto initialization is enabled
        
        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            if(autoInit) Init();
        }
        
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            _characterCollider = GetComponent<CircleCollider2D>();
            _firstPosition = transform.position;
            _targetPosition = transform.position;
            AddEvent();
            SetRigidbody();
            if(DemoControl.Instance) DemoControl.Instance.OnPlayMode -= CheckMode;
            if(DemoControl.Instance) DemoControl.Instance.OnPlayMode += CheckMode;
        }

        /// <summary>
        /// 모드 설정 확인
        /// Check mode settings
        /// </summary>
        /// <param name="playMode">플레이 모드 / Play mode</param>
        private void CheckMode(PlayMode playMode)
        {
            switch (playMode)
            {
                case PlayMode.Home: Reset(); break;
                case PlayMode.Experience: OnMove(); break;
                case PlayMode.None: break;
                default: throw new ArgumentOutOfRangeException(nameof(playMode), playMode, null);
            }
        }
        
        /// <summary>
        /// 리지드바디 설정
        /// Set rigidbody properties
        /// </summary>
        private void SetRigidbody()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        /// <summary>
        /// 콜라이더 활성화/비활성화 설정
        /// Set collider enable/disable
        /// </summary>
        /// <param name="isOn">활성화 여부 / Enable status</param>
        public void SetCollider(bool isOn)
        {
            _characterCollider.enabled = isOn;
        }
        

        /// <summary>
        /// 초기 위치로 리셋
        /// Reset to initial position
        /// </summary>
        public void Reset()
        {
            if (!_canMove) return;
            _canMove = false;
            _isMoving = true;
            _targetPosition = _firstPosition;
        }

        /// <summary>
        /// 이동 가능 상태로 설정
        /// Set to movable state
        /// </summary>
        public void OnMove()
        {
            _canMove = true;
        }

        private void Update()
        {
            if(_rigidbody == null) 
            {
                Debug.LogWarning("No rigidbody attached to player");
                return;
            }
            
            if (_isMoving && PlayerState != EPlayerState.Run)
            {
                PlayerState = EPlayerState.Run;
                PartsManager.PlayAnimation("Run");
            }
            else if (!_isMoving && PlayerState != EPlayerState.Idle)
            {
                PlayerState = EPlayerState.Idle;
                PartsManager.PlayAnimation("Idle");
            }
        }

        private void FixedUpdate()
        {
            if (joystick.Value != Vector2.zero)
            {
                Vector2 input = joystick.Value;
                Vector3 pos = Vector3.zero;
                var frameMovement = input * Time.deltaTime * 2f;
                transform.position += Vector3.right * frameMovement.x;
                transform.position += Vector3.up * frameMovement.y;
                
                var x = PartsManager.transform.localScale.x;
                if (input.x < 0 && x < 0 || (input.x > 0 && x > 0))
                {
                    PartsManager.transform.localScale = input.x < 0 ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
                }
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }
        }

        
        /// <summary>
        /// 현재 위치와 목표 위치 사이의 거리 계산
        /// Calculate distance between current location and target location
        /// </summary>
        /// <returns>거리 / Distance</returns>
        private float InputDistance()
        {
            return (_targetPosition - transform.position).magnitude;
        }

        /// <summary>
        /// 스파인 이벤트 추가
        /// Add spine event
        /// </summary>
        private void AddEvent()
        {
            PartsManager.GetSkeletonAnimation().AnimationState.Event -= HandleEvent;
            PartsManager.GetSkeletonAnimation().AnimationState.Event += HandleEvent;
        }

        /// <summary>
        /// 스파인 이벤트 처리
        /// Handle spine events
        /// </summary>
        /// <param name="trackEntry">트랙 엔트리 / Track entry</param>
        /// <param name="e">이벤트 / Event</param>
        private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == _footStepEventName)
            {
                AudioManager.Instance.PlayStepSound();
            }
        }

        private void OnDestroy()
        {
            if(DemoControl.Instance) DemoControl.Instance.OnPlayMode -= CheckMode;
            
            if (PartsManager)
            {
                PartsManager.GetSkeletonAnimation().AnimationState.Event -= HandleEvent;
            }
        }
    }
}