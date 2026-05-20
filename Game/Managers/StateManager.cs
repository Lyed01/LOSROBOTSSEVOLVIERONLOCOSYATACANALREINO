using ProyectoSDL2.Game.Interfaces;

namespace ProyectoSDL2.Game.Managers
{
    public class StateManager
    {
        private IGameState _current;

        public void ChangeState(IGameState newState)
        {
            _current?.Exit();
            _current = newState;
            _current.Enter();
        }

        public void Update(float dt) => _current?.Update(dt);
        public void Render()         => _current?.Render();
    }
}
