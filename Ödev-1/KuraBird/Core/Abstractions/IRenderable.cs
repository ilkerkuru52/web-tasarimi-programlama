namespace KuraBird.Core.Abstractions
{
    /// <summary>
    /// Soyutlama (Abstraction): Çizilebilir nesneler için arayüz.
    /// </summary>
    public interface IRenderable
    {
        void Render(Graphics g);
    }
}
