using UnityEngine;

namespace Pluto.Actors
{
    /// <summary>
    /// 공격 테스트를 위한 샌드백 객체.
    /// Actor를 상속받아 데미지 처리 로직을 가집니다.
    /// </summary>
    public class EnemyDummy : Actor
    {
        [Header("Dummy Settings")]
        [SerializeField] private Color _hitColor = Color.red;
        [SerializeField] private float _colorFlashDuration = 0.1f;

        private MeshRenderer _renderer;
        private Color _originalColor;

        protected virtual void Start()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
        }

        /// <summary>
        /// 데미지를 받았을 때의 연출 및 로그 출력
        /// </summary>
        public override void TakeDamage(float amount)
        {
            base.TakeDamage(amount);
            
            // 피격 시 색상 변경 연출 코루틴 실행
            if (_renderer != null)
            {
                StopAllCoroutines();
                StartCoroutine(HitFlashCoroutine());
            }
        }

        private System.Collections.IEnumerator HitFlashCoroutine()
        {
            _renderer.material.color = _hitColor;
            yield return new WaitForSeconds(_colorFlashDuration);
            _renderer.material.color = _originalColor;
        }
    }
}
