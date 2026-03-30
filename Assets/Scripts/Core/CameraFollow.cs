using UnityEngine;

namespace Pluto.Core
{
    /// <summary>
    /// 타겟(보통 플레이어)을 따라다니는 카메라 로직.
    /// 쿼터뷰 시점을 위해 고정된 오프셋을 유지합니다.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform target;
        
        [Header("Settings")]
        public Vector3 offset = new Vector3(0f, 18f, -15f);
        [SerializeField] private float _smoothTime = 0.2f;

        private Vector3 _currentVelocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            // 목표 좌표 계산
            Vector3 targetPosition = target.position + offset;
            
            // 부드러운 이동 (SmoothDamp)
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);
        }
    }
}
