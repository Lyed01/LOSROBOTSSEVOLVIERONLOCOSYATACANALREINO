using SDL2;
using System.Diagnostics;
using ProyectoSDL2.Game.Managers;
using ProyectoSDL2.Game.States;
using E = ProyectoSDL2.Engine.Engine;

namespace ProyectoSDL2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ventana fisica 1280x960, dibujada en coordenadas logicas 1024x768.
            // SDL escala el render x1.25 para llenar la ventana sin barras (4:3 exacto).
            E.Initialize(1280, 960, 1024, 768);

            var stateManager = new StateManager();
            stateManager.ChangeState(new MenuState(stateManager));

            bool running = true;
            var sw = Stopwatch.StartNew();
            float lastTime = 0f;

            while (running)
            {
                float now = (float)sw.Elapsed.TotalSeconds;
                float dt = System.Math.Min(now - lastTime, 0.05f);
                lastTime = now;

                // Chequeamos quit sin remover eventos de la cola
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        running = false;

                    // ── Atajos de debug (flanco de bajada, una vez por pulsacion) ──
                    // F1/F2/F3: saltar a un mapa (mandan al arbol antes de ese nivel).
                    // F12: muchos recursos. Ocultos: no se muestran en pantalla.
                    if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.repeat == 0)
                    {
                        var gm = GameManager.Instance;
                        switch (e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_F1:   // Mapa 1 (nivel 1)
                                gm.IrANivel(1);
                                stateManager.ChangeState(new SkillTreeState(stateManager));
                                break;
                            case SDL.SDL_Keycode.SDLK_F2:   // Mapa 2 (nivel 2)
                                gm.IrANivel(2);
                                stateManager.ChangeState(new SkillTreeState(stateManager));
                                break;
                            case SDL.SDL_Keycode.SDLK_F3:   // Mapa 3 (nivel 4)
                                gm.IrANivel(4);
                                stateManager.ChangeState(new SkillTreeState(stateManager));
                                break;
                            case SDL.SDL_Keycode.SDLK_F12:  // Recursos infinitos
                                gm.DarRecursosDebug();
                                break;

                            // Volumen de la musica: + sube, - baja (en cualquier pantalla)
                            case SDL.SDL_Keycode.SDLK_PLUS:
                            case SDL.SDL_Keycode.SDLK_EQUALS:
                            case SDL.SDL_Keycode.SDLK_KP_PLUS:
                                MusicaManager.SubirVolumen();
                                break;
                            case SDL.SDL_Keycode.SDLK_MINUS:
                            case SDL.SDL_Keycode.SDLK_KP_MINUS:
                                MusicaManager.BajarVolumen();
                                break;
                        }
                    }
                }

                if (E.KeyPress(E.KEY_ESC))
                    running = false;

                stateManager.Update(dt);
                stateManager.Render();
            }

            SDL.SDL_Quit();
        }
    }
}