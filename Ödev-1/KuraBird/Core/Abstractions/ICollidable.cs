namespace KuraBird.Core.Abstractions
{
    /// <summary>
    /// Soyutlama (Abstraction): Çarpışma algılayabilen nesneler için arayüz.
    /// </summary>
    public interface ICollidable
    {
        RectangleF GetBounds();
        void OnCollision();
    }
}
