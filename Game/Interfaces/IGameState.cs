namespace ProyectoSDL2.Game.Interfaces
{
    public interface IGameState
    {
        void Enter();
        void Update(float dt);
        void Render();
        void Exit();
    }
}
