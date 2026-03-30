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
        [SerializeField] private float _dashDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 1.0f;
        [SerializeField] private int _maxDashCharges = 2;

        private StatHandler _statHandler;
        private PlayerCombat _combat;
        private Vector2 _moveInput;
        private Camera _mainCamera;

        private int _currentDashCharges;
        private float _dashCooldownTimer;
        private bool _isDashing;

        protected override void Awake()
        {
            base.Awake();
            _statHandler = GetComponent<StatHandler>();
            _combat = GetComponent<PlayerCombat>();
            _mainCamera = Camera.main;
            
            _currentDashCharges = _maxDashCharges;

            if (_mainCamera == null)
            {
                Debug.LogError("PlayerController: Main Camera not found!");
            }
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
        /// InputSystem_Actions에서 호출되는 이동 입력 이벤트.
        /// </summary>
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        /// <summary>
        /// Space Bar 입력 시 대시 발동.
        /// </summary>
        public void OnJump(InputValue value)
        {
            if (_currentDashCharges > 0 && !_isDashing)
            {
                StartCoroutine(DashCoroutine());
            }
        }

        private System.Collections.IEnumerator DashCoroutine()
        {
            _isDashing = true;
            IsInvincible = true;
            _currentDashCharges--;
            
            // 첫 대시 시작 시 쿨다운 타이머 시작
            if (_currentDashCharges == _maxDashCharges - 1)
            {
                _dashCooldownTimer = _dashCooldown;
            }

            // 이동 입력이 있으면 그 방향으로, 없으면 현재 보는 방향으로 대시
            Vector3 dashDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            if (dashDir == Vector3.zero) dashDir = transform.forward;

            float startTime = Time.time;
            while (Time.time < startTime + _dashDuration)
            {
                Rb.linearVelocity = dashDir * _dashSpeed;
                yield return null;
            }

            _isDashing = false;
            IsInvincible = false;
        }

        private void FixedUpdate()
        {
            if (!_isDashing)
            {
                ApplyMovement();
            }
            ApplyRotation();
        }

        private void ApplyMovement()
        {
            // StatHandler가 없으면 기본값(10) 사용
            float currentSpeed = _statHandler != null ? _statHandler.GetValue(_moveSpeedType) : 10f;
            
            // 공격 중에는 이동 속도 대폭 감소 (0.2배)
            if (_combat != null && _combat.IsAttacking)
            {
                currentSpeed *= 0.2f;
            }

            // 입력 벡터를 3D 공간으로 변환
            Vector3 targetVelocity = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * currentSpeed;
            
            // Rigidbody 리니어 속도 직접 제어
            Rb.linearVelocity = new Vector3(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.z);
        }

        private void ApplyRotation()
        {
            // 마우스 왼쪽 버튼을 클릭 중이거나, 공격 동작 중일 때 회전 처리
            bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool isAttacking = _combat != null && _combat.IsAttacking;

            if (!isMousePressed && !isAttacking) return;

            // 마우스 정보를 얻기 위해 레이캐스트 활용
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            
            // 바닥(보통 0 좌표)과의 충돌 여부를 확인하기 위해 간단한 Plane 사용 또는 Physics.Raycast 사용
            // 여기서는 단순함을 위해 Ground 레이어 또는 Plane과의 인터섹션을 활용
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            
            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 lookPoint = ray.GetPoint(entry);
                Vector3 direction = (lookPoint - transform.position).normalized;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Rb.MoveRotation(targetRotation);
                }
            }
        }
    }
}
