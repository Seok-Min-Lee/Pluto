using UnityEngine;
using Pluto.Interfaces;

namespace Pluto.Core
{
    /// <summary>
    /// 일직선으로 이동하며 충돌 시 데미지를 입히는 투사체 스크립트.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _lifetime;
        private LayerMask _targetLayer;
        private bool _isInitialized = false;

        /// <summary>
        /// 투사체의 속성 초기화 및 발사
        /// </summary>
        public void Initialize(Vector3 direction, float speed, float damage, float lifetime, LayerMask targetLayer)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _lifetime = lifetime;
            _targetLayer = targetLayer;
            _isInitialized = true;

            // 투사체 방향에 맞춰 회전 설정
            if (_direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            // 지정된 수명 이후 소멸
            Destroy(gameObject, _lifetime);
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // 매 프레임 이동 처리
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isInitialized)
            {
                return;
            }

            // 1. 레이어 마스크 필터링
            if (((1 << other.gameObject.layer) & _targetLayer) != 0)
            {
                // 2. 데미지 전달
                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_damage);
                    Debug.Log($"<color=magenta>[Pluto Magic]</color> Projectile hit <b>{other.name}</b> for <b>{_damage}</b> damage!");
                }
                
                // 3. 충돌 시 소멸
                Destroy(gameObject);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                // 벽 등 지형 충돌 시에도 소멸
                Destroy(gameObject);
            }
        }
    }
}
