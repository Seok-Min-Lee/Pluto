using UnityEngine;
using UnityEngine.InputSystem;
using Pluto.Core;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 이동 및 회전을 제어하는 액터 클래스.
    /// WASD 이동과 마우스 커서 조준을 실시간으로 처리합니다.
    /// </summary>
    public class PlayerController : Actor
    {
        [Header("Movement Settings")]
        [SerializeField] private StatType _moveSpeedType = StatType.MoveSpeed;
        
        [Header("Dash Settings")]
        [SerializeField] private float _dashSpeed = 30f;
        /// <summary>
        /// 대시의 지속 시간입니다. (애니메이션 길이에 맞춰 0.3초로 동기화 완료)
        /// </summary>
        [SerializeField] private float _dashDuration = 0.3f;
        /// <summary>
        /// 대시 이동이 끝난 후 발생하는 경직 시간입니다. (동작의 무게감을 위해 0.15초 추가)
        /// </summary>
        [SerializeField] private float _dashRecoveryDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 1.0f;
        [SerializeField] private int _maxDashCharges = 2;

        private StatHandler _statHandler;
        private PlayerCombat _combat;
        private PlayerView _view;
        private Vector2 _moveInput;
        public Vector2 MoveInput => _moveInput; // Recovery Cancel 감지용 프로퍼티 노출
        private Camera _mainCamera;

        private int _currentDashCharges;
        private float _dashCooldownTimer;
        
        [SerializeField] private float _rotationSpeed = 15.0f;
        private bool _isDashing;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            _view = GetComponent<PlayerView>();
            _combat = GetComponent<PlayerCombat>();
            _statHandler = GetComponent<StatHandler>();

            if (Rb != null)
            {
                // 물리 보간 활성화를 통해 고주사율 모니터에서의 이동 끊김(Stuttering) AUTHORITATIVLY 해결
                Rb.interpolation = RigidbodyInterpolation.Interpolate;
                Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // 물리적 충돌로 인한 0.1mm 단위의 원치 않는 회전 뒤틀림을 AUTHORITATIVLY 방지
                Rb.freezeRotation = true;
            }

            _currentDashCharges = _maxDashCharges;
        }

        private void Update()
        {
            HandleDashCooldown();
        }

        private void HandleDashCooldown()
        {
            if (_currentDashCharges < _maxDashCharges)
            {
                _dashCooldownTimer -= Time.deltaTime;
                if (_dashCooldownTimer <= 0)
                {
                    _currentDashCharges++;
                    if (_currentDashCharges < _maxDashCharges)
                    {
                        _dashCooldownTimer = _dashCooldown;
                    }
                }
            }
        }

        /// <summary>
        /// Input System의 Move 액션 이벤트 핸들러.
        /// </summary>
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
            
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                Debug.Log($"[Pluto] OnMove Received: {_moveInput}");
            }
        }

        /// <summary>
        /// Space Bar 입력 시 대시 발동. (Dash Action)
        /// </summary>
        public void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            if (_currentDashCharges <= 0 || _isDashing)
            {
                return;
            }

            // 공격 중이라면 공격을 취소하고 대시 실행 (Dash Cancel)
            if (_combat != null && _combat.IsAttacking)
            {
                _combat.CancelAttack();
            }

            StartCoroutine(DashCoroutine());
        }

private System.Collections.IEnumerator DashCoroutine()
        {
            _isDashing = true;
            IsInvincible = true;
            _currentDashCharges--;
            
            // 물리 저항을 일시적으로 무력화하여 추진력을 100% 보존
            float originalDrag = Rb.linearDamping;
            Rb.linearDamping = 0f;

            if (_currentDashCharges == _maxDashCharges - 1)
            {
                _dashCooldownTimer = _dashCooldown;
            }

            Vector3 dashDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            if (dashDir == Vector3.zero)
            {
                dashDir = transform.forward;
            }

            if (_view != null) 
            {
                _view.PlayDash();
            }

            // Time.fixedTime을 사용하여 물리 엔진 스텝과 동기화
            float startFixedTime = Time.fixedTime;
            var wait = new WaitForFixedUpdate();
            
            while (Time.fixedTime < startFixedTime + _dashDuration)
            {
                // 방향 보간 (Slerp 유지)
                if (dashDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dashDir), _rotationSpeed * Time.fixedDeltaTime);
                }

                // 이전의 linearVelocity 직접 제어 방식으로 원복
                Vector3 velocity = dashDir * _dashSpeed;
                velocity.y = Rb.linearVelocity.y;
                Rb.linearVelocity = velocity;
                
                yield return wait;
            }

            // 물리 저항 원상 복구 및 상태 해제
            Rb.linearDamping = originalDrag;
            
            // 대시 종료 시 속도 초기화
            Rb.linearVelocity = Vector3.zero;
            
            yield return new WaitForSeconds(_dashRecoveryDuration);

            _isDashing = false;
            IsInvincible = false;
        }

        private void FixedUpdate()
        {
            // 대시 중이거나 공격 중일 때는 키보드 이동/회전 입력을 원천 차단하여 물리적 일관성을 확보합니다.
            bool isBusy = _isDashing || (_combat != null && _combat.IsAttacking);
            
            if (!isBusy)
            {
                ApplyMovement();
                ApplyRotation();
            }
            
            UpdateAnimation();
        }



        private void UpdateAnimation()
        {
            if (_view != null)
            {
                // 수평 이동 속도만 애니메이션에 반영 (Y축 제외)
                Vector3 horizontalVel = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
                _view.UpdateMoveSpeed(horizontalVel.magnitude);
            }
        }

        private void ApplyMovement()
        {
            // StatHandler가 없으면 기본값(10) 사용
            float currentSpeed = _statHandler != null ? _statHandler.GetStatValue(_moveSpeedType) : 10f;
            
            // 공격 중에는 이동 속도 대폭 감소 (0.2배)
            if (_combat != null && _combat.IsAttacking)
            {
                currentSpeed *= 0.2f;
            }

            // 입력 벡터를 3D 공간으로 변환
            Vector3 targetVelocity = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * currentSpeed;
            
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                Debug.Log($"[Pluto] Moving: Speed={currentSpeed}, Input={_moveInput}, TargetVel={targetVelocity}");
            }
            
            // Rigidbody 리니어 속도 직접 제어
            Rb.linearVelocity = new Vector3(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.z);
        }

        /// <summary>
        /// 캐릭터의 회전을 제어합니다. 마우스 입력/공격 시 혹은 이동 방향에 맞춰 회전합니다.
        /// </summary>
        private void ApplyRotation()
        {
            // 1. 마우스 조준 또는 공격 중인지 확인 (조준 우선권)
            bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool isAttacking = _combat != null && _combat.IsAttacking;

            if (isMousePressed || isAttacking)
            {
                RotateToCursor();
                return;
            }

            // 2. 일반 이동 중인지 확인 (8방향 회전)
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                RotateToMoveDirection();
            }
        }


        /// <summary>
        /// 마우스 커서 방향으로 플레이어를 회전시킵니다.
        /// </summary>
        private void RotateToCursor()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 lookPoint = ray.GetPoint(entry);
                Vector3 direction = (lookPoint - transform.position).normalized;
                direction.y = 0; // 수평 회전만 허용

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
                }
            }
        }

        /// <summary>
        /// 이동 입력 방향(8방향)으로 플레이어를 회전시킵니다.
        /// </summary>
        private void RotateToMoveDirection()
        {
            Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
        }

    }
}
