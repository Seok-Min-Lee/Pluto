using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Pluto.Core;
using Pluto.Interfaces;


namespace Pluto.Actors
{
    /// <summary>
    /// 공격 데이터 구조체 (콤보 및 특수 공격 설정용)
    /// </summary>
    [System.Serializable]
    public struct AttackData
    {
        public float Duration;
        public float DashForce;
        public float RecoveryDuration;
    }

    /// <summary>
    /// 플레이어 공격 시스템 관리 클래스
    /// </summary>
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
        private PlayerController _controller;
        private int _comboIndex = 0;
        private float _lastAttackTime;
        private bool _isAttacking;
        private bool _inputBuffered;

        private const float RecoveryCancelThreshold = 0.1f;

        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// 주요 컴포넌트 참조 초기화
        /// </summary>
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
        /// 마우스 커서 위치 기반 조준 방향 계산
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
                direction.y = 0;
                
                if (direction != Vector3.zero)
                {
                    return direction;
                }
            }

            return transform.forward;
        }

        /// <summary>
        /// Input System 일반 공격(마우스 왼쪽) 이벤트 핸들러
        /// </summary>
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                HandleAttackInput();
            }
        }

        /// <summary>
        /// Input System 특수 공격(마우스 오른쪽) 이벤트 핸들러
        /// </summary>
        public void OnSpecial(InputValue value)
        {
            if (value.isPressed && !_isAttacking)
            {
                StartCoroutine(SpecialAttackCoroutine());
            }
        }

        /// <summary>
        /// Input System 매직 공격(Q) 이벤트 핸들러
        /// </summary>
        public void OnMagic(InputValue value)
        {
            if (value.isPressed && !_isAttacking)
            {
                StartCoroutine(MagicAttackCoroutine());
            }
        }

        /// <summary>
        /// Input System 신성 원조(R) 이벤트 핸들러
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
        /// Input System 상호작용(E) 이벤트 핸들러
        /// </summary>
        public void OnInteract(InputValue value)
        {
            // 디버그용 로그 추가 (입력 감지 확인)
            Debug.Log($"<color=cyan>[Pluto Input]</color> Interact Key Pressed! Value: {value.isPressed}");

            if (value.isPressed && _controller != null)
            {
                // 1. 주변의 상호작용 가능 객체 탐색
                IInteractable target = _controller.GetNearestInteractable();
                
                if (target != null)
                {
                    Debug.Log($"<color=green>[Pluto Interaction]</color> Interacting with <b>{target.GetType().Name}</b>");
                    
                    // 2. 상호작용 애니메이션 재생
                    if (_view != null) 
                    {
                        _view.PlayInteract();
                    }
                    
                    // 3. 상호작용 실행
                    target.Interact();
                }
                else
                {
                    Debug.Log("<color=gray>[Pluto Interaction]</color> No interactable object nearby.");
                }
            }
        }

        /// <summary>
        /// 현재 플레이어 전방의 대상을 탐색하여 데미지를 입힙니다.
        /// </summary>
        private void PerformHitDetection()
        {
            // 1. 판정 중심점 계산
            Vector3 hitPoint = transform.position + transform.TransformDirection(_hitOffset);

            // 2. 범위 내의 모든 콜라이더 탐색
            Collider[] targets = Physics.OverlapSphere(hitPoint, _attackRange, _targetLayer);
            
            foreach (var col in targets)
            {
                // 3. IDamageable 인터페이스 확인
                if (col.TryGetComponent<IDamageable>(out var damageable))
                {
                    // 4. 스태츠 기반 데미지 계산 (현재는 기본값 전달)
                    damageable.TakeDamage(_attackDamage);
                    Debug.Log($"<color=red>[Pluto Combat]</color> Hit <b>{col.name}</b> for <b>{_attackDamage}</b> damage!");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 공격 판정 범위 시각화
            Gizmos.color = Color.red;
            Vector3 hitPoint = transform.position + transform.TransformDirection(_hitOffset);
            Gizmos.DrawWireSphere(hitPoint, _attackRange);
        }


        /// <summary>
        /// 공격 동작 즉시 중단 및 상태 리셋
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
        }

        private void HandleAttackInput()
        {
            if (_isAttacking)
            {
                _inputBuffered = true;
                return;
            }
            StartCoroutine(AttackCoroutine());
        }

        /// <summary>
        /// 콤보 공격 프로세스 코루틴
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            // 1. 공격 상태 시작 및 콤보 정보 갱신
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

            // 2. 조준 방향 기준 회전 스냅
            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlayAttack(_comboIndex);
            }

            // 3. 통합 메서드를 이용한 마이크로 대시 수행
            if (_controller != null) 
            {   
                _controller.SetLinearVelocity(attackDir, currentAttack.DashForce);
            }

            // [추가] 공격 판정 타이밍 (액션 지속 시간의 20% 지점에서 판정 발생)
            yield return new WaitForSeconds(currentAttack.Duration * 0.2f);
            PerformHitDetection();

            // 4. 남은 공격 액션 지속 시간 대기
            yield return new WaitForSeconds(currentAttack.Duration * 0.8f);

            // 5. 복구(Recovery) 구간 진입 및 입력 버퍼링 감지
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

            // 6. 상태 종료 및 연쇄 공격 처리
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

        /// <summary>
        /// 특수 공격 프로세스 코루틴
        /// </summary>
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

            if (_controller != null)
            {
                _controller.SetLinearVelocity(attackDir, _specialAttack.DashForce);
            }

            // [추가] 공격 판정 타이밍
            yield return new WaitForSeconds(_specialAttack.Duration * 0.3f);
            PerformHitDetection();

            // 1. 남은 공격 액션 구간 대기
            yield return new WaitForSeconds(_specialAttack.Duration * 0.7f);

            // 2. 복구 구간 온전 대기
            yield return new WaitForSeconds(_specialAttack.RecoveryDuration);

            _isAttacking = false;
        }

        /// <summary>
        /// 매직 공격 프로세스 코루틴
        /// </summary>
        /// <summary>
        /// 매직 공격 프로세스 코루틴
        /// </summary>
        private IEnumerator MagicAttackCoroutine()
        {
            _isAttacking = true;

            // 1. 조준 방향 기준 회전 스냅
            Vector3 attackDir = GetAimDirection();
            if (attackDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDir);
            }

            if (_view != null)
            {
                _view.PlayMagic();
            }

            // 2. 투사체 발사 타이밍 (애니메이션 시작 직후)
            if (_magicProjectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.TransformDirection(_magicSpawnOffset);
                GameObject projObj = Instantiate(_magicProjectilePrefab, spawnPos, Quaternion.LookRotation(attackDir));
                
                if (projObj.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Initialize(attackDir, _magicProjectileSpeed, _magicProjectileDamage, _magicProjectileLifetime, _targetLayer);
                }
            }

            if (_controller != null)
            {
                _controller.SetLinearVelocity(attackDir, _magicAttack.DashForce);
            }

            // 3. 공격 액션 구간 대기
            yield return new WaitForSeconds(_magicAttack.Duration);

            // 4. 복구 구간 대기
            yield return new WaitForSeconds(_magicAttack.RecoveryDuration);

            _isAttacking = false;
        }
    


        [Header("Combat Settings")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackAngle = 120f;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Vector3 _hitOffset = new Vector3(0, 0, 0.5f);



        [Header("Magic Settings")]
        [SerializeField] private GameObject _magicProjectilePrefab;
        [SerializeField] private float _magicProjectileSpeed = 15f;
        [SerializeField] private float _magicProjectileDamage = 25f;
        [SerializeField] private float _magicProjectileLifetime = 3f;
        [SerializeField] private Vector3 _magicSpawnOffset = new Vector3(0, 1f, 0.5f);
}
}
