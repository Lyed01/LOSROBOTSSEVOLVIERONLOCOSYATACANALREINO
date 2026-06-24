using ProyectoSDL2.Engine;
using ProyectoSDL2.Game;
using System;
using System.Collections.Generic;
using SDL2;

namespace ProyectoSDL2.Game.Entities
{
    // Torre hachera (melee): cada 1/cadencia segundos pega un golpe al suelo que
    // daña a TODOS los enemigos en las 8 casillas adyacentes (incluidas diagonales)
    // mas la propia, es decir un area de 3x3 tiles centrada en la torre.
    // Cada golpe dispara una animacion de impacto (onda expansiva + flash).
    public class AxeTower : Tower
    {
        // Animacion de golpe (frames de sprite, se dibuja ARRIBA de la torre).
        // En cada golpe se reproduce UNA sola vez la secuencia completa en orden.
        private Image[] _framesAtaque;          // frames del golpe (puede ser null)
        private float   _animTimer  = 0f;       // tiempo restante de la animacion
        private const float DURACION_ANIM   = 0.4f;

        // Mejora "Onda Expansiva": agranda el area de golpe de 3x3 a 5x5
        private bool _ondaExpansiva = false;
        public void ActivarOndaExpansiva() => _ondaExpansiva = true;

        public AxeTower(int x, int y, Image sheet, Image[] framesAtaque = null)
            : base(x, y, sheet)
        {
            _framesAtaque = framesAtaque;
            Rango    = 3 * Map.TILE;   // referencia: alcanza el area 3x3
            Dano     = 50f;
            cadencia = 0.5f;           // 1 golpe cada 2 segundos
            totalFrames   = 2;
            intervalFrame = 0.1f;
        }

        public override void Attack() { }

        public override void Update(float dt, List<Enemy> enemies, GestorDeBalas balas)
        {
            timerAtaque += dt;
            atacando     = false;

            if (timerAtaque >= 1f / cadencia)
            {
                (int ax, int ay, int aw, int ah) = AreaGolpe();
                bool golpeo = false;

                // Dañar a todos los enemigos dentro del area 3x3 (con diagonales)
                foreach (var e in enemies)
                {
                    if (!e.IsAlive) continue;
                    if (e.Aereo) continue;   // el hachero no alcanza a los voladores
                    if (e.CollidesWith(ax, ay, aw, ah))
                    {
                        e.TakeDamage((int)Dano);
                        golpeo = true;
                    }
                }

                // Solo golpea (y anima) si habia algun enemigo en el area
                if (golpeo)
                {
                    atacando    = true;
                    timerAtaque = 0f;
                    Attack();

                    // Arrancar la animacion del golpe (se reproduce una vez)
                    _animTimer = DURACION_ANIM;
                }
            }

            // Consumir el tiempo de la animacion mientras dure
            if (_animTimer > 0f)
                _animTimer -= dt;

            UpdateAnimation(dt);
        }

        public override void Render()
        {
            // La torre SIEMPRE se dibuja con su sprite.
            base.Render();

            // La animacion del golpe se dibuja ENCIMA de la torre (desde el centro
            // de la casilla), sin reemplazar el sprite de la torre. Recorre los
            // frames en orden (0..N-1) UNA sola vez durante la duracion del golpe.
            if (_animTimer > 0f && _framesAtaque != null && _framesAtaque.Length > 0)
            {
                float prog = 1f - (_animTimer / DURACION_ANIM);   // 0 -> 1
                int idx = (int)(prog * _framesAtaque.Length);
                if (idx >= _framesAtaque.Length) idx = _framesAtaque.Length - 1;
                DibujarAnimacionGolpe(_framesAtaque[idx]);
            }
        }

        // Rectangulo centrado en la casilla: 3x3 normal, 5x5 con Onda Expansiva.
        private (int, int, int, int) AreaGolpe()
        {
            int radio = _ondaExpansiva ? 2 : 1;            // en tiles
            int lado  = (2 * radio + 1) * Map.TILE;
            int ax    = X - radio * Map.TILE;
            int ay    = Y - radio * Map.TILE;
            return (ax, ay, lado, lado);
        }

        // Dibuja el frame de la animacion centrado en el CENTRO de la casilla.
        private void DibujarAnimacionGolpe(Image img)
        {
            int w = 160, h = 160;   // 25% mas grande que el sprite de la torre (128)
            int x = X + (Map.TILE / 2) - (w / 2);
            int y = Y + (Map.TILE / 2) - (h / 2);   // centrado en el centro del tile
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = x, y = y, w = w, h = h };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, img.Pointer, IntPtr.Zero, ref dest);
        }
    }
}
