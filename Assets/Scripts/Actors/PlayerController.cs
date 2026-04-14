using UnityEngine;
using UnityEngine.InputSystem;
using Pluto.Core;
using Pluto.Core;


namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어 이동 및 회전 제어 클래스
    /// </summary>
    public class PlayerController : Actor
    {
        [Header("Movement Settings")]
        [SerializeField] private StatType _moveSpeedType = StatType.MoveSpeed;
        
        [Header("Dash Settings")]
        [SerializeField] private float _dashSpeed = 30f;
        /// <summary>
        /// 대시 지속 시간 (애니메이션 길이에 동기화)
        /// </summary>
        [SerializeField] private float _dashDuration = 0.3f;
        /// <summary>
        /// 대시 종료 후 후딜레이 시간
        /// </summary>
        [SerializeField] private float _dashRecoveryDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 1.0f;
        [SerializeField] private int _maxDashCharges = 2;

        private StatHandler _statHandler;
        private PlayerCombat _combat;
        private PlayerView _view;
        private Vector2 _moveInput;
        public Vector2 MoveInput => _moveInput; // Recovery Cancel 감지용 프로퍼티
        private Camera _mainCamera;

        private int _currentDashCharges;
        private float _dashCooldownTimer;
        
        [SerializeField] private float _rotationSpeed = 15.0f;
        private bool _isDashing;

        /// <summary>
        /// 컴포넌트 참조 초기화 및 물리 설정
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _view = GetComponent<PlayerView>();
            _combat = GetComponent<PlayerCombat>();
            _statHandler = GetComponent<StatHandler>();
            _mainCamera = Camera.main;

            _currentDashCharges = _maxDashCharges;
        }

        /// <summary>
        /// 선속도 물리적 설정 (Y축 속도 유지)
        /// </summary>
        /// <param name="direction">이동 방향</param>
        /// <param name="speed">이동 속도</param>
        public void SetLinearVelocity(Vector3 direction, float speed)
        {
            Vector3 velocity = direction.normalized * speed;
            velocity.y = Rb.linearVelocity.y;
            Rb.linearVelocity = velocity;
        }

        private void Update()
        {
            HandleDashCooldown();
        }

        /// <summary>
        /// 대시 충전 쿨다운 관리
        /// </summary>
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
        /// Input System 이동 입력 이벤트 핸들러
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
        /// Input System 대시 입력 이벤트 핸들러
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

            // 공격 중일 경우 공격 취소 (Dash Cancel)
            if (_combat != null && _combat.IsAttacking)
            {
                _combat.CancelAttack();
            }

            StartCoroutine(DashCoroutine());
        }

        /// <summary>
        /// 주변의 가장 가까운 상호작용 가능 객체 탐색
        /// </summary>
        /// <returns>탐색된 IInteractable 객체 (없으면 null)</returns>
        public IInteractable GetNearestInteractable()
        {
            // 1. 플레이어 반경 내의 모든 콜라이더 탐색
            Collider[] colliders = Physics.OverlapSphere(transform.position, _interactionRange);
            IInteractable nearest = null;
            float minDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                // 2. IInteractable 인터페이스를 포함하는지 확인
                if (col.TryGetComponent<IInteractable>(out var interactable))
                {
                    // 3. 상호작용 가능 상태인 경우에만 거리 계산
                    if (interactable.CanInteract())
                    {
                        float dist = Vector3.Distance(transform.position, col.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearest = interactable;
                        }
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 에디터 씬 뷰에서 상호작용 범위를 시각화
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }



        /// <summary>
        /// 대시 물리 로직 코루틴
        /// </summary>
        private System.Collections.IEnumerator DashCoroutine()
        {
            // 1. 대시 상태 시작 및 스탯 설정
            _isDashing = true;
            IsInvincible = true;
            _currentDashCharges--;
            
            // 2. 물리 저항 일시 무력화 (추진력 보존)
            float originalDrag = Rb.linearDamping;
            Rb.linearDamping = 0f;

            if (_currentDashCharges == _maxDashCharges - 1)
            {
                _dashCooldownTimer = _dashCooldown;
            }

            // 3. 대시 방향 결정
            Vector3 dashDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            if (dashDir == Vector3.zero)
            {
                dashDir = transform.forward;
            }

            if (_view != null) 
            {
                _view.PlayDash();
            }

            // 4. 대시 지속 주입 루프 (FixedUpdate 동기화)
            float startFixedTime = Time.fixedTime;
            var wait = new WaitForFixedUpdate();

            while (Time.fixedTime < startFixedTime + _dashDuration)
            {
                // 방향 보간 (Slerp)
                if (dashDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dashDir), _rotationSpeed * Time.fixedDeltaTime);
                }

                // 통합 속도 제어 메서드 호출
                SetLinearVelocity(dashDir, _dashSpeed);

                yield return wait;
            }

            // 5. 물리 저항 복구 및 대시 종료
            Rb.linearDamping = originalDrag;
            Rb.linearVelocity = Vector3.zero;
            
            yield return new WaitForSeconds(_dashRecoveryDuration);

            _isDashing = false;
            IsInvincible = false;
        }

        /// <summary>
        /// 물리 이동 및 회전 업데이트
        /// </summary>
        private void FixedUpdate()
        {
            // 대시나 공격 중에는 일반 이동 입력 차단
            bool isBusy = _isDashing || (_combat != null && _combat.IsAttacking);
            
            if (!isBusy)
            {
                ApplyMovement();
                ApplyRotation();
            }
            
            UpdateAnimation();
        }

        /// <summary>
        /// 현재 속도 기반 애니메이션 파라미터 업데이트
        /// </summary>
        private void UpdateAnimation()
        {
            if (_view != null)
            {
                Vector3 horizontalVel = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
                _view.UpdateMoveSpeed(horizontalVel.magnitude);
            }
        }

        /// <summary>
        /// 입력 방향에 따른 실제 물리 이동 적용
        /// </summary>
        private void ApplyMovement()
        {
            float currentSpeed = _statHandler != null ? _statHandler.GetStatValue(_moveSpeedType) : 10f;
            
            // 공격 중 이동 속도 감쇄 처리
            if (_combat != null && _combat.IsAttacking)
            {
                currentSpeed *= 0.2f;
            }

            Vector3 targetVelocity = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * currentSpeed;
            
            // Rigidbody 직접 속도 제어
            Rb.linearVelocity = new Vector3(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.z);
        }

        /// <summary>
        /// 입력/공격 상황별 회전 우선순위 적용
        /// </summary>
        private void ApplyRotation()
        {
            bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool isAttacking = _combat != null && _combat.IsAttacking;

            // 1. 공격 또는 마우스 입력 방향 우선
            if (isMousePressed || isAttacking)
            {
                RotateToCursor();
                return;
            }

            // 2. 이동 입력 방향 차선책
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                RotateToMoveDirection();
            }
        }

        /// <summary>
        /// 마우스 커서 월드 좌표 방향 회전 처리
        /// </summary>
        private void RotateToCursor()
        {
            if (Mouse.current == null || _mainCamera == null) 
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 lookPoint = ray.GetPoint(entry);
                Vector3 direction = (lookPoint - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
                }
            }
        }

        /// <summary>
        /// 이동 입력 벡터 방향 회전 처리
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
    


        [Header("Interaction Settings")]
        [SerializeField] private float _interactionRange = 2.0f;
}
}
