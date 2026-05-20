namespace ProyectoSDL2.Game.Interfaces
{
    public interface IAttacker
    {
        float Rango { get; }
        float Dano  { get; }
        void  Attack();
    }
}
