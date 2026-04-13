using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 공격 시스템을 담당하는 클래스.
    /// 3단 콤보와 입력 버퍼링, 마이크로 대시 기능을 제공합니다.
    /// </summary>
    [System.Serializable]
    public struct AttackData
    {
        public float Duration;
        public float DashForce;
        public float RecoveryDuration;
    }

    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combo Settings")]
        [SerializeField] private float _comboResetTime = 1.0f;
        [SerializeField] private AttackData[] _comboAttacks = 
        {
            new AttackData { Duration = 0.3f, DashForce = 12f, RecoveryDuration = 0.15f },
            new AttackData { Duration = 0.3f, DashForce = 14f, RecoveryDuration = 0.15f },
            new AttackData { Duration = 0.45f, DashForce = 18f, RecoveryDuration = 0.2f }
        };

        [Header("Special/Magic Settings")]
        [SerializeField] private AttackData _specialAttack = new AttackData { Duration = 0.5f, DashForce = 18f, RecoveryDuration = 0.2f };
        [SerializeField] private AttackData _magicAttack = new AttackData { Duration = 0.2f, DashForce = 5f, RecoveryDuration = 0.1f };

        private Rigidbody _rb;
        private Camera _mainCamera;
        private PlayerView _view;
        private PlayerController _controller; // 이동 입력 감지용 레퍼런스 추가
        private int _comboIndex = 0;
        private float _lastAttackTime;
        private bool _isAttacking;
        private bool _inputBuffered;

        // 조작 임계값 상수화 (Rule 3-2 준수)
        private const float RecoveryCancelThreshold = 0.1f;

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _view = GetComponent<PlayerView>();
            _controller = GetComponent<PlayerController>();
            _mainCamera = Camera.main;
            
            if (_view == null)
            {
                Debug.LogWarning("PlayerCombat: PlayerView component not found! Animations may not play.");
            }

            if (_controller == null)
            {
                Debug.LogError("PlayerCombat: PlayerController component not found! Recovery Cancel will not work.");
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
            if (_comboIndex > _comboAttacks.Length)
            {
                _comboIndex = 1;
            }

            _lastAttackTime = Time.time;
            AttackData currentAttack = _comboAttacks[_comboIndex - 1];

            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlayAttack(_comboIndex);
            }

            // [원복] 1회성 속입 주입 및 WaitForSeconds 대기 방식으로 원상 복구
            Vector3 velocity = attackDir * currentAttack.DashForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            // 공격 액션 구간 온전 대기
            yield return new WaitForSeconds(currentAttack.Duration);

            // 5. 복구 구간 진입 및 입력 감지 루프
            int recState = _comboIndex == 1 ? PlayerView.AttackARecState : PlayerView.AttackBRecState;
            if (_comboIndex < 3 && _view != null)
            {
                _view.PlayAction(recState);
            }

            float elapsed = 0f;
            while (elapsed < currentAttack.RecoveryDuration)
            {
                if (_inputBuffered)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            _isAttacking = false;

            if (_comboIndex == _comboAttacks.Length)
            {
                _inputBuffered = false;
            }

            if (_inputBuffered)
            {
                HandleAttackInput();
            }
        }



        private IEnumerator SpecialAttackCoroutine()
        {
            _isAttacking = true;

            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlaySpecial();
            }

            Vector3 velocity = attackDir * _specialAttack.DashForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            // 1. 공격 액션 구간 온전 대기
            yield return new WaitForSeconds(_specialAttack.Duration);

            // 2. 복구 구간 온전 대기
            yield return new WaitForSeconds(_specialAttack.RecoveryDuration);

            _isAttacking = false;
        }

        private IEnumerator MagicAttackCoroutine()
        {
            _isAttacking = true;

            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlayMagic();
            }

            Vector3 velocity = attackDir * _magicAttack.DashForce;
            velocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocity;

            // 1. 공격 액션 구간 온전 대기
            yield return new WaitForSeconds(_magicAttack.Duration);

            // 2. 복구 구간 온전 대기
            yield return new WaitForSeconds(_magicAttack.RecoveryDuration);

            _isAttacking = false;
        }
    }
}
