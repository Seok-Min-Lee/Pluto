using UnityEngine;
using Pluto.Interfaces;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어와 적의 공통 기능을 담은 추상 베이스 클래스.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Actor : MonoBehaviour, IDamageable
    {
        [Header("Actor References")]
        public Rigidbody Rb { get; protected set; }

        [Header("Actor States")]
        public bool IsInvincible { get; protected set; }

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            
            // 물리 이동을 위한 기본 설정
            Rb.linearDamping = 5f;
            Rb.angularDamping = 0.05f;
            Rb.freezeRotation = true; // 액션 게임에서 회전은 스크립트로 직접 제어
        }

        /// <summary>
        /// 데미지를 입었을 때의 기본 처리. 무적 상태일 경우 무시합니다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        public virtual void TakeDamage(float amount)
        {
            if (IsInvincible)
            {
                return;
            }

            Debug.Log($"{gameObject.name} took {amount} damage.");
        }
    }
}
