namespace KuraBird.Core.Abstractions
{
    /// <summary>
    /// Soyutlama (Abstraction): Güncellenebilir nesneler için arayüz.
    /// </summary>
    public interface IUpdatable
    {
        void Update(float deltaTime);
    }
}
