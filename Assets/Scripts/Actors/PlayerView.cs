using UnityEngine;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 시각적 연출(애니메이션, 이펙트 등)을 전담하는 View 계층 클래스.
    /// 스크립트 기반의 CrossFade를 사용하여 정밀한 애니메이션 전이를 제어합니다.
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        private Animator _animator;
        
        // --- Animator 파라미터 ID (Blend Tree 및 전역 파라미터) ---
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");

        // --- Animator 상태 Hash (CrossFade용) ---
        public static readonly int LocomotionState = Animator.StringToHash("Locomotion");
        public static readonly int AttackAState = Animator.StringToHash("Attack A");
        public static readonly int AttackBState = Animator.StringToHash("Attack B");
        public static readonly int AttackCState = Animator.StringToHash("Attack C");
        public static readonly int AttackARecState = Animator.StringToHash("Attack A Rec");
        public static readonly int AttackBRecState = Animator.StringToHash("Attack B Rec");
        public static readonly int AttackSpecialState = Animator.StringToHash("Attack Special");
        public static readonly int MagicState = Animator.StringToHash("Magic");
        public static readonly int DashState = Animator.StringToHash("Dash");
        public static readonly int HitState = Animator.StringToHash("Hit");
        public static readonly int DeathState = Animator.StringToHash("Death");

        // 공격 단계별 상태 배열 (리팩토링: 인덱스 기반 접근)
        private static readonly int[] AttackStates = { AttackAState, AttackBState, AttackCState };

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator != null)
            {
                return;
            }
            
            Debug.LogError($"[Pluto View] Animator not found on {gameObject.name}");
        }

        /// <summary>
        /// 특정 애니메이션 상태로 즉시 전이합니다. (CrossFade)
        /// </summary>
        /// <param name="stateHash">전이할 상태의 해시값</param>
        /// <param name="transitionDuration">전이 소요 시간 (초)</param>
        public void PlayAction(int stateHash, float transitionDuration = 0.1f)
        {
            _animator.CrossFadeInFixedTime(stateHash, transitionDuration);
        }

        /// <summary>
        /// 이동 속도를 애니메이터(Blend Tree)에 전달합니다.
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
        /// 단계별 공격 애니메이션을 재생합니다.
        /// </summary>
        public void PlayAttack(int comboIndex)
        {
            int index = Mathf.Clamp(comboIndex - 1, 0, AttackStates.Length - 1);
            PlayAction(AttackStates[index]);
        }

        public void PlaySpecial() => PlayAction(AttackSpecialState);
        public void PlayMagic() => PlayAction(MagicState);
        public void PlayHit() => PlayAction(HitState);
        public void PlayDash() => PlayAction(DashState, 0.05f); // 대시는 더 정밀하게 즉시 전이
        public void PlayDie() => PlayAction(DeathState);

        /// <summary>
        /// 상호작용 애니메이션을 재생합니다.
        /// </summary>
        public void PlayInteract() => PlayAction(InteractState);

        
        /// <summary>
        /// 소모품 사용 등 기타 트리거용 (필요 시 CrossFade로 확장 가능)
        /// </summary>
        public void PlayConsume()
        {
            _animator.SetTrigger(Animator.StringToHash("Consume"));
        }
    

        public static readonly int InteractState = Animator.StringToHash("Interact");
}
}
