using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;
using SDL2;

namespace ProyectoSDL2.Game.States
{
    public class VictoryState : IGameState
    {
        private StateManager _sm;
        private Font  _fontGrande;
        private Font  _fontChica;
        private Image _fondo;

        public VictoryState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            _fondo      = Engine.Engine.LoadImage("Assets/MainMenu.png");
            _fontGrande = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 56);
            _fontChica  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 26);
        }

        public void Update(float dt)
        {
            if (Engine.Engine.KeyPress(SDL.SDL_Keycode.SDLK_RETURN) ||
                Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _))
            {
                GameManager.Instance.ResetGame();
                _sm.ChangeState(new MenuState(_sm));
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref dest);
            Engine.Engine.DrawText("¡VICTORIA!",                                                        330, 260, 100, 255, 100, _fontGrande);
            Engine.Engine.DrawText($"Puntaje: {GameManager.Instance.PuntajeAcumulado}",                390, 360, 255, 215,   0, _fontChica);
            Engine.Engine.DrawText($"Monedas: {GameManager.Instance.Monedas}",                         390, 400, 255, 215,   0, _fontChica);
            Engine.Engine.DrawText("ENTER para volver al menú",                                         300, 500, 180, 180, 180, _fontChica);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }

    public class DefeatState : IGameState
    {
        private StateManager _sm;
        private Font  _fontGrande;
        private Font  _fontChica;
        private Image _fondo;

        public DefeatState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            _fondo      = Engine.Engine.LoadImage("Assets/PantallaDerrota.png");
            _fontGrande = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 56);
            _fontChica  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 26);
        }

        public void Update(float dt)
        {
            if (Engine.Engine.KeyPress(SDL.SDL_Keycode.SDLK_RETURN) ||
                Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _))
            {
                GameManager.Instance.ResetGame();
                _sm.ChangeState(new MenuState(_sm));
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref dest);
            Engine.Engine.DrawText("GAME OVER",                                                         340, 260, 255,  60,  60, _fontGrande);
            Engine.Engine.DrawText("El reino de Stonehaven ha caído",                                   270, 340, 200, 150, 150, _fontChica);
            Engine.Engine.DrawText($"Puntaje final: {GameManager.Instance.PuntajeAcumulado}",           380, 400, 255, 215,   0, _fontChica);
            Engine.Engine.DrawText("ENTER para volver al menú",                                         300, 500, 180, 180, 180, _fontChica);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }
}
