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
        /// Input System의 Attack 액션 이벤트 핸들러.
        /// </summary>
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                HandleAttackInput();
            }
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
            if (_comboIndex > 3) _comboIndex = 1;

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
    }
}
