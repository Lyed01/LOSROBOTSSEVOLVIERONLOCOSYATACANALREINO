namespace ProyectoSDL2.Game.Interfaces
{
    public interface IDamageable
    {
        int  Health  { get; }
        bool IsAlive { get; }
        void TakeDamage(int amount);
    }
}
