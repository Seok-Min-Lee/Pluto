using UnityEngine;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 시각적 연출(애니메이션, 이펙트 등)을 전담하는 View 계층 클래스.
    /// DIV 아키텍처의 View 역할을 수행하며, Controller/Combat으로부터 데이터를 전달받아 Animator를 제어합니다.
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        private Animator _animator;
        
        // Animator 파라미터 ID 최적화 (해시값 사용)
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
        private static readonly int SpecialHash = Animator.StringToHash("Special");
        private static readonly int MagicHash = Animator.StringToHash("Magic");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int ConsumeHash = Animator.StringToHash("Consume");
        private static readonly int DashHash = Animator.StringToHash("Dash");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError($"[Pluto View] Animator not found on {gameObject.name}");
            }
        }

        /// <summary>
        /// 이동 속도를 애니메이터에 전달합니다.
        /// </summary>
        public void UpdateMoveSpeed(float speed)
        {
            _animator.SetFloat(MoveSpeedHash, speed);
        }

        /// <summary>
        /// 캐릭터의 대화 상태를 토글합니다.
        /// </summary>
        public void SetTalking(bool isTalking)
        {
            _animator.SetBool(IsTalkingHash, isTalking);
        }

        /// <summary>
        /// 공격 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayAttack(int comboIndex)
        {
            _animator.SetInteger(ComboIndexHash, comboIndex);

            if (comboIndex != 1)
            {
                return;
            }

            _animator.SetTrigger(AttackHash);
        }

        /// <summary>
        /// 특수 공격 애니메이션을 트리거합니다.
        /// </summary>
        public void PlaySpecial()
        {
            _animator.SetTrigger(SpecialHash);
        }

        /// <summary>
        /// 마법 공격 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayMagic()
        {
            _animator.SetTrigger(MagicHash);
        }

        /// <summary>
        /// 피격 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayHit()
        {
            _animator.SetTrigger(HitHash);
        }

        /// <summary>
        /// 보상 획득(Consume) 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayDash()
        {
            _animator.SetTrigger(DashHash);
        }

        /// <summary>
        /// 사망 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayDie()
        {
            _animator.SetTrigger(DieHash);
        }

        /// <summary>
        /// 보상 획득(Consume) 애니메이션을 트리거합니다.
        /// </summary>
        public void PlayConsume()
        {
            _animator.SetTrigger(ConsumeHash);
        }
    }
}
