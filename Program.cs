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
            E.Initialize(1024, 768);

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