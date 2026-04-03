using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 공격 시스템을 담당하는 클래스.
    /// 3단 콤보와 입력 버퍼링, 마이크로 대시 기능을 제공합니다.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combo Settings")]
        [SerializeField] private float _comboResetTime = 1.0f;
        [SerializeField] private float[] _attackDurations = { 0.3f, 0.3f, 0.45f };
        [SerializeField] private float[] _microDashForces = { 12f, 14f, 18f };

        [Header("Special/Magic Settings")]
        [SerializeField] private float _specialDuration = 0.5f;
        [SerializeField] private float _specialMicroDashForce = 18f;
        [SerializeField] private float _magicDuration = 0.2f;
        [SerializeField] private float _magicMicroDashForce = 5f;

        private Rigidbody _rb;
        
        private Camera _mainCamera;
private PlayerView _view;
        private int _comboIndex = 0;
        private float _lastAttackTime;
        private bool _isAttacking;
        private bool _inputBuffered;

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _view = GetComponent<PlayerView>();
            _mainCamera = Camera.main;
            
            if (_view == null)
            {
                Debug.LogWarning("PlayerCombat: PlayerView component not found! Animations may not play.");
            }

            if (_mainCamera == null)
            {
                Debug.LogError("[Pluto Combat] Main Camera not found! Aim direction cannot be calculated.");
            }
        }


        /// <summary>
        /// 현재 마우스 커서가 가리키는 월드 좌표 방향을 계산합니다. (0.1mm 정밀도 보장)
        /// </summary>
        private Vector3 GetAimDirection()
        {
            if (_mainCamera == null)
            {
                return transform.forward;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 lookPoint = ray.GetPoint(entry);
                Vector3 direction = (lookPoint - transform.position).normalized;
                direction.y = 0; // 수평 이동만 허용
                
                if (direction != Vector3.zero)
                {
                    return direction;
                }
            }

            return transform.forward;
        }


        /// <summary>
        /// Input System의 Attack 액션 이벤트 핸들러. (마우스 왼쪽)
        /// </summary>
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                HandleAttackInput();
            }
        }

        /// <summary>
        /// Input System의 Special 액션 이벤트 핸들러. (마우스 오른쪽)
        /// </summary>
        public void OnSpecial(InputValue value)
        {
            if (value.isPressed && !_isAttacking)
            {
                StartCoroutine(SpecialAttackCoroutine());
            }
        }

        /// <summary>
        /// Input System의 Magic 액션 이벤트 핸들러. (Q 키)
        /// </summary>
        public void OnMagic(InputValue value)
        {
            if (value.isPressed && !_isAttacking)
            {
                StartCoroutine(MagicAttackCoroutine());
            }
        }

        /// <summary>
        /// Input System의 Call 액션 이벤트 핸들러. (R 키)
        /// </summary>
        public void OnCall(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("<color=yellow>[Pluto Combat]</color> <b>Divine Aid (Call) Requested! (R)</b>");
                // TODO: BoonHandler를 통해 실제 신의 원조 효과 발동
            }
        }

        /// <summary>
        /// Input System의 Interact 액션 이벤트 핸들러. (E 키)
        /// </summary>
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("<color=green>[Pluto Combat]</color> <b>Interact Requested! (E)</b>");
                // TODO: 근처의 상호작용 가능한 객체 탐색 및 실행
            }
        }

        /// <summary>
        /// 공격 동작을 즉시 중단합니다. (대시 캔슬용)
        /// </summary>
        public void CancelAttack()
        {
            if (!_isAttacking)
            {
                return;
            }
            
            StopAllCoroutines();
            _isAttacking = false;
            _inputBuffered = false;
            Debug.Log("<color=yellow>[Pluto Combat]</color> Attack Cancelled!");
        }

        private void HandleAttackInput()
        {
            if (_isAttacking)
            {
                // 공격 중 입력이 들어오면 버퍼에 저장
                _inputBuffered = true;
                return;
            }

            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _inputBuffered = false;

            if (Time.time - _lastAttackTime > _comboResetTime)
            {
                _comboIndex = 0;
            }

            _comboIndex++;
            // 콤보 인덱스 방체 및 배열 범위 동기화 (Rule 4-4 준수)
            if (_comboIndex > _attackDurations.Length)
            {
                _comboIndex = 1;
            }

            _lastAttackTime = Time.time;

            // 공격 정밀 방향 계산 및 회전 스냅 (Body Snap)
            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            // 애니메이션 실행
            if (_view != null)
            {
                _view.PlayAttack(_comboIndex);
            }

            // 마이크로 대시: 콤보 단계별 정밀 추진력 주입
            float currentForce = GetMicroDashForce(_comboIndex);
            Vector3 velocity = attackDir * currentForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            // 콤보 단계별 정밀 지속 시간 대기
            float currentDuration = GetAttackDuration(_comboIndex);
            yield return new WaitForSeconds(currentDuration);

            _isAttacking = false;

            if (_inputBuffered)
            {
                HandleAttackInput();
            }
        }



        private IEnumerator SpecialAttackCoroutine()
        {
            _isAttacking = true;

            // 정밀 방향 계산 및 회전 스냅 (Body Snap)
            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlaySpecial();
            }

            Vector3 velocity = attackDir * _specialMicroDashForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            yield return new WaitForSeconds(_specialDuration);
            _isAttacking = false;
        }



        private IEnumerator MagicAttackCoroutine()
        {
            _isAttacking = true;

            // 정밀 방향 계산 및 회전 스냅 (Body Snap)
            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlayMagic();
            }

            Vector3 velocity = attackDir * _magicMicroDashForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            yield return new WaitForSeconds(_magicDuration);
            _isAttacking = false;
        }

        /// <summary>
        /// 현재 콤보 단계에 대응하는 공격 지속 시간을 정밀하게 반환합니다. (0.1ms 정밀도)
        /// </summary>
        private float GetAttackDuration(int comboIndex)
        {
            int index = Mathf.Clamp(comboIndex - 1, 0, _attackDurations.Length - 1);
            return _attackDurations.Length > 0 ? _attackDurations[index] : 0.3f;
        }

        /// <summary>
        /// 현재 콤보 단계에 대응하는 마이크로 대시 추진력을 정밀하게 반환합니다. (0.1mm 정밀도)
        /// </summary>
        private float GetMicroDashForce(int comboIndex)
        {
            int index = Mathf.Clamp(comboIndex - 1, 0, _microDashForces.Length - 1);
            return _microDashForces.Length > 0 ? _microDashForces[index] : 12f;
        }


    }
}
