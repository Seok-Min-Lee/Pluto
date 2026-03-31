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
        [SerializeField] private float _attackDuration = 0.3f;
        [SerializeField] private float _microDashForce = 12f;

        [Header("Special/Magic Settings")]
        [SerializeField] private float _specialDuration = 0.5f;
        [SerializeField] private float _specialMicroDashForce = 18f;
        [SerializeField] private float _magicDuration = 0.2f;
        [SerializeField] private float _magicMicroDashForce = 5f;

        private Rigidbody _rb;
        private int _comboIndex = 0;
        private float _lastAttackTime;
        private bool _isAttacking;
        private bool _inputBuffered;

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
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

            // 콤보 리셋 체크 (마지막 공격으로부터 일정 시간이 지나면 1타부터 시작)
            if (Time.time - _lastAttackTime > _comboResetTime)
            {
                _comboIndex = 0;
            }

            _comboIndex++;
            if (_comboIndex > 3)
            {
                _comboIndex = 1;
            }

            Debug.Log($"<color=red>[Pluto Combat]</color> Attack Phase: <b>{_comboIndex}</b>");
            _lastAttackTime = Time.time;

            // 마이크로 대시: 공격 방향으로 짧고 강하게 전진
            Vector3 attackDir = transform.forward;
            _rb.linearVelocity = attackDir * _microDashForce;

            // 공격 동작 시간 (추후 애니메이션 프레임과 동기화 필요)
            yield return new WaitForSeconds(_attackDuration);

            _isAttacking = false;

            // 입력 버퍼가 있다면 다음 공격 실행
            if (_inputBuffered)
            {
                HandleAttackInput();
            }
        }

        private IEnumerator SpecialAttackCoroutine()
        {
            _isAttacking = true;
            Debug.Log("<color=orange>[Pluto Combat]</color> <b>Special Attack! (Mouse Right)</b>");

            Vector3 attackDir = transform.forward;
            _rb.linearVelocity = attackDir * _specialMicroDashForce;

            yield return new WaitForSeconds(_specialDuration);
            _isAttacking = false;
        }

        private IEnumerator MagicAttackCoroutine()
        {
            _isAttacking = true;
            Debug.Log("<color=cyan>[Pluto Combat]</color> <b>Magic Cast! (Q)</b>");

            Vector3 attackDir = transform.forward;
            _rb.linearVelocity = attackDir * _magicMicroDashForce;

            yield return new WaitForSeconds(_magicDuration);
            _isAttacking = false;
        }
    }
}
