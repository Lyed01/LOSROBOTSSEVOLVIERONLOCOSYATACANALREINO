using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;

namespace ProyectoSDL2.Game.States
{
    public class VictoryState : IGameState
    {
        private StateManager _sm;
        private Font _fontGrande;
        private Font _fontChica;

        public VictoryState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            _fontGrande = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 56);
            _fontChica  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 26);
        }

        public void Update(float dt)
        {
            if (Engine.Engine.KeyPress(SDL2.SDL.SDL_Keycode.SDLK_RETURN) ||
                Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _))
            {
                GameManager.Instance.ResetGame();
                _sm.ChangeState(new MenuState(_sm));
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            Engine.Engine.DrawText("¡VICTORIA!",                     330, 260, 100, 255, 100, _fontGrande);
            Engine.Engine.DrawText($"Puntaje: {GameManager.Instance.PuntajeAcumulado}", 390, 360, 255, 215,   0, _fontChica);
            Engine.Engine.DrawText($"Monedas: {GameManager.Instance.Monedas}",          390, 400, 255, 215,   0, _fontChica);
            Engine.Engine.DrawText("ENTER para volver al menú",      300, 500, 180, 180, 180, _fontChica);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }

    public class DefeatState : IGameState
    {
        private StateManager _sm;
        private Font _fontGrande;
        private Font _fontChica;

        public DefeatState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            _fontGrande = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 56);
            _fontChica  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 26);
        }

        public void Update(float dt)
        {
            if (Engine.Engine.KeyPress(SDL2.SDL.SDL_Keycode.SDLK_RETURN) ||
                Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _))
            {
                GameManager.Instance.ResetGame();
                _sm.ChangeState(new MenuState(_sm));
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            Engine.Engine.DrawText("GAME OVER",                      340, 260, 255,  60,  60, _fontGrande);
            Engine.Engine.DrawText("El reino de Stonehaven ha caido", 270, 340, 200, 150, 150, _fontChica);
            Engine.Engine.DrawText($"Puntaje final: {GameManager.Instance.PuntajeAcumulado}", 380, 400, 255, 215, 0, _fontChica);
            Engine.Engine.DrawText("ENTER para volver al menú",      300, 500, 180, 180, 180, _fontChica);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }
}
