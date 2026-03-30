using UnityEngine;
using UnityEngine.InputSystem;
using Pluto.Core;

namespace Pluto.Actors
{
    /// <summary>
    /// 플레이어의 이동 및 회전을 제어하는 액터 클래스.
    /// WASD 이동과 마우스 커서 조준을 실시간으로 처리합니다.
    /// </summary>
    public class PlayerController : Actor
    {
        [Header("Movement Settings")]
        [SerializeField] private StatType _moveSpeedType = StatType.MoveSpeed;
        
        private StatHandler _statHandler;
        private Vector2 _moveInput;
        private Camera _mainCamera;

        protected override void Awake()
        {
            base.Awake();
            _statHandler = GetComponent<StatHandler>();
            _mainCamera = Camera.main;
            
            if (_mainCamera == null)
            {
                Debug.LogError("PlayerController: Main Camera not found!");
            }
        }

        /// <summary>
        /// InputSystem_Actions에서 호출되는 이동 입력 이벤트.
        /// </summary>
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyRotation();
        }

        private void ApplyMovement()
        {
            // StatHandler가 없으면 기본값(10) 사용
            float currentSpeed = _statHandler != null ? _statHandler.GetValue(_moveSpeedType) : 10f;
            
            // 입력 벡터를 3D 공간으로 변환 (Isometric 기준에서는 카메라 각도에 맞춘 회전이 필요할 수 있으나, 일단 월드 좌표계 사용)
            Vector3 targetVelocity = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * currentSpeed;
            
            // Rigidbody 리니어 속도 직접 제어 (Unity 6+)
            Rb.linearVelocity = new Vector3(targetVelocity.x, Rb.linearVelocity.y, targetVelocity.z);
        }

        private void ApplyRotation()
        {
            // 마우스 왼쪽 버튼을 클릭/홀드 중일 때만 회전 처리
            if (Mouse.current == null || !Mouse.current.leftButton.isPressed) return;

            // 마우스 정보를 얻기 위해 레이캐스트 활용
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            
            // 바닥(보통 0 좌표)과의 충돌 여부를 확인하기 위해 간단한 Plane 사용 또는 Physics.Raycast 사용
            // 여기서는 단순함을 위해 Ground 레이어 또는 Plane과의 인터섹션을 활용
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            
            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 lookPoint = ray.GetPoint(entry);
                Vector3 direction = (lookPoint - transform.position).normalized;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Rb.MoveRotation(targetRotation);
                }
            }
        }
    }
}
