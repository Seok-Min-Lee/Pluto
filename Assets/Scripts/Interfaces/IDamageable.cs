namespace Pluto.Interfaces
{
    /// <summary>
    /// 데미지를 입을 수 있는 모든 개체의 공통 인터페이스.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 데미지를 처리하는 메서드.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        void TakeDamage(float amount);
    }
}
