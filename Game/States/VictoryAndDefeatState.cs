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

        // Transiciones de fundido. Al entrar aparece desde negro (el popup
        // "emerge"); al continuar se funde a negro antes de ir al menu.
        private float _fadeIn   = 1f;     // 1 = negro total, 0 = escena visible
        private bool  _saliendo = false;
        private float _fadeOut  = 0f;     // 0 = escena visible, 1 = negro total
        private const float FADE_VEL = 1.6f;   // velocidad del fundido (por segundo)

        public VictoryState(StateManager sm) => _sm = sm;

        public void Enter()
        {
            _fondo      = Engine.Engine.LoadImage("Assets/MainMenu.png");
            _fontGrande = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 56);
            _fontChica  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 26);

            MusicaManager.Reproducir(MusicaManager.VICTORIA);
        }

        public void Update(float dt)
        {
            // Fundido de entrada
            if (_fadeIn > 0f)
                _fadeIn = System.Math.Max(0f, _fadeIn - FADE_VEL * dt);

            if (!_saliendo)
            {
                // Solo se puede continuar una vez que termino el fundido de entrada
                if (_fadeIn <= 0f &&
                    (Engine.Engine.KeyPress(SDL.SDL_Keycode.SDLK_RETURN) ||
                     Engine.Engine.MouseClick(Engine.Engine.MOUSE_LEFT, out _, out _)))
                {
                    _saliendo = true;
                }
            }
            else
            {
                // Fundido de salida hacia la proxima pantalla (menu)
                _fadeOut += FADE_VEL * dt;
                if (_fadeOut >= 1f)
                {
                    GameManager.Instance.ResetGame();
                    _sm.ChangeState(new MenuState(_sm));
                }
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref dest);

            // ── Popup centrado ────────────────────────────────────────────────
            IntPtr r = Engine.Engine.renderer;
            SDL.SDL_SetRenderDrawBlendMode(r, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            int pw = 520, ph = 320;
            int px = (1024 - pw) / 2;
            int py = (768  - ph) / 2;

            // Fondo oscuro semitransparente del panel
            SDL.SDL_SetRenderDrawColor(r, 20, 25, 35, 230);
            SDL.SDL_Rect panel = new SDL.SDL_Rect { x = px, y = py, w = pw, h = ph };
            SDL.SDL_RenderFillRect(r, ref panel);

            // Borde verde de victoria
            SDL.SDL_SetRenderDrawColor(r, 100, 255, 100, 255);
            SDL.SDL_RenderDrawRect(r, ref panel);

            CentrarTexto("¡VICTORIA!", py + 40, 100, 255, 100, _fontGrande);
            CentrarTexto($"Puntaje: {GameManager.Instance.PuntajeAcumulado}", py + 140, 255, 215, 0, _fontChica);
            CentrarTexto($"Oro: {GameManager.Instance.Monedas}",             py + 180, 255, 215, 0, _fontChica);
            CentrarTexto("ENTER para volver al menú", py + 250, 180, 180, 180, _fontChica);

            // ── Overlay de fundido (entrada y salida) ─────────────────────────
            float alpha = _saliendo ? _fadeOut : _fadeIn;
            if (alpha > 0f)
            {
                byte a = (byte)System.Math.Min(255f, alpha * 255f);
                SDL.SDL_SetRenderDrawColor(r, 0, 0, 0, a);
                SDL.SDL_Rect full = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
                SDL.SDL_RenderFillRect(r, ref full);
            }

            Engine.Engine.Show();
        }

        // Dibuja un texto centrado horizontalmente en la pantalla a la altura y.
        private void CentrarTexto(string texto, int y, byte r, byte g, byte b, Font font)
        {
            int w = Engine.Engine.TextWidth(texto, font);
            Engine.Engine.DrawText(texto, (1024 - w) / 2, y, r, g, b, font);
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

            MusicaManager.Reproducir(MusicaManager.DERROTA);
        }

        public void Update(float dt)
        {
            // Reintentar: reinicia el nivel actual (en planeacion), conserva el progreso
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_R))
            {
                GameManager.Instance.RepetirNivel();
                _sm.ChangeState(new PlanningState(_sm));
            }

            // Reiniciar partida: vuelve al menu desde cero
            if (Engine.Engine.KeyPress(SDL.SDL_Keycode.SDLK_RETURN))
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
            Engine.Engine.DrawText("R: repetir nivel",                                                  360, 470, 180, 220, 180, _fontChica);
            Engine.Engine.DrawText("ENTER: reiniciar partida (menu)",                                   300, 510, 180, 180, 180, _fontChica);
            Engine.Engine.DrawText("ESC: salir del juego",                                              350, 550, 200, 160, 160, _fontChica);
            Engine.Engine.Show();
        }

        public void Exit() { }
    }
}
