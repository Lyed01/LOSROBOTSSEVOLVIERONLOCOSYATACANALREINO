using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;

namespace ProyectoSDL2.Game.States
{
    public class MenuState : IGameState
    {
        private StateManager _sm;
        private Font  _fontTitulo;
        private Font  _fontSub;
        private Image _fondo;

        public MenuState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            string ruta = System.IO.Path.GetFullPath("Assets/Fonts/pixel.ttf");
            Engine.Engine.Debug("Buscando fuente en: " + ruta);
            Engine.Engine.Debug("Existe: " + System.IO.File.Exists(ruta).ToString());
            _fontTitulo = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 40);
            _fontSub = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 28);
        }

        public void Update(float dt)
        {
            // Click o ENTER → empezar
            if (Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _) ||
                Engine.Engine.KeyPress(SDL2.SDL.SDL_Keycode.SDLK_RETURN))
            {
                GameManager.Instance.ResetGame();
                _sm.ChangeState(new PlanningState(_sm));
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            // if (_fondo != null) Engine.Engine.Draw(_fondo, 0, 0);

            string linea1 = "LOS ROBOTS SE VOLVIERON LOCOS";
            string linea2 = "Y ATACAN AL REINO";
            string linea3 = "ENTER para jugar";

            int screenW = 1024;

            Engine.Engine.DrawText(linea1, (screenW - Engine.Engine.TextWidth(linea1, _fontTitulo)) / 2, 150, 255, 220, 50, _fontTitulo);
            Engine.Engine.DrawText(linea2, (screenW - Engine.Engine.TextWidth(linea2, _fontTitulo)) / 2, 210, 255, 220, 50, _fontTitulo);
            Engine.Engine.DrawText(linea3, (screenW - Engine.Engine.TextWidth(linea3, _fontSub)) / 2, 400, 200, 200, 200, _fontSub);
            Engine.Engine.DrawText("Paradigmas de Programacion 2026", 280, 700, 150, 150, 150, _fontSub);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }
}
