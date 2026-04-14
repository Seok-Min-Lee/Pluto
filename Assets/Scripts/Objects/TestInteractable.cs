using UnityEngine;
using Pluto.Core;

namespace Pluto.Objects
{
    /// <summary>
    /// 상호작용 시스템 테스트용 클래스
    /// </summary>
    public class TestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _message = "Hello, I am an interactable object!";
        private bool _isUsed = false;

        public void Interact()
        {
            Debug.Log($"<color=cyan>[Test Interactable]</color> <b>{_message}</b>");
            _isUsed = true;
        }

        public bool CanInteract()
        {
            // 한 번만 상호작용 가능하도록 설정 (테스트용)
            return !_isUsed;
        }
    }
}
