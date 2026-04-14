namespace Pluto.Core
{
    /// <summary>
    /// 상호작용 가능한 객체를 위한 인터페이스
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용 발동 시 실행될 로직
        /// </summary>
        void Interact();
        
        /// <summary>
        /// 상호작용 가능 여부 확인 (조건부 상호작용 대응)
        /// </summary>
        bool CanInteract();
    }
}
