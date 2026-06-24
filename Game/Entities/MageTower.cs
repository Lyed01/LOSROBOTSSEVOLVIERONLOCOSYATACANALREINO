using ProyectoSDL2.Engine;
using ProyectoSDL2.Game;
using ProyectoSDL2.Game.Managers;
using System.Collections.Generic;
using SDL2;

namespace ProyectoSDL2.Game.Entities
{
    // Torre de mago: lanza un haz de fuego recto de 3 tiles en una direccion.
    // El haz se prende y apaga ciclicamente y daña por ticks a los enemigos que toca.
    public class MageTower : Tower
    {
        private Direccion _dir;
        public  Direccion Dir => _dir;   // direccion del haz (para dibujar su area)

        // Animacion del haz: serie de frames de fuego (puede ser null = respaldo)
        private Image[] _fuegoFrames;
        private int     _fuegoIdx   = 0;
        private float   _fuegoTimer = 0f;
        private const float FUEGO_INTERVALO = 0.1f;

        // Ciclo prendido/apagado
        private float _cicloTimer = 0f;
        private bool  _hazActivo  = false;
        private const float T_ON  = 1.5f;  
        private const float T_OFF = 1.0f;   

        // Daño por ticks mientras el haz esta encendido
        private float _tickTimer = 0f;
        private const float TICK = 0.3f;

        // Ralentizacion: factor de velocidad que aplica a los enemigos del haz
        // (1 = sin efecto). Lo configura la TowerFactory segun el arbol.
        private float _slowFactor = 1f;
        public void ConfigurarRalentizacion(float factor) { _slowFactor = factor; }

        // Mejora "Haz Continuo": el haz no se apaga nunca.
        private bool _hazPermanente = false;
        public void ActivarHazPermanente() => _hazPermanente = true;

        public MageTower(int x, int y, Image sheet, Direccion dir, Image[] fuego = null)
            : base(x, y, sheet)
        {
            _dir         = dir;
            _fuegoFrames = fuego;
            Rango  = 3 * Map.TILE;
            Dano   = 8f;          // daño por tick
            cadencia = 1f;        // no se usa, el ciclo es propio
            totalFrames   = 2;
            intervalFrame = 0.2f;
        }

        public override void Attack() { }

        public override void Update(float dt, List<Enemy> enemies, GestorDeBalas balas)
        {
            _cicloTimer += dt;

            if (_hazPermanente)
            {
                _hazActivo = true;   // con la mejora el haz nunca se apaga
            }
            else if (_hazActivo)
            {
                if (_cicloTimer >= T_ON) { _hazActivo = false; _cicloTimer = 0f; }
            }
            else
            {
                if (_cicloTimer >= T_OFF) { _hazActivo = true; _cicloTimer = 0f; _tickTimer = 0f; }
            }

            // Daño por ticks y animacion del fuego mientras el haz esta encendido
            if (_hazActivo)
            {
                _tickTimer += dt;
                if (_tickTimer >= TICK)
                {
                    _tickTimer = 0f;
                    DanarEnArea(enemies);
                }

                // Avanzar el frame del fuego ciclicamente
                if (_fuegoFrames != null && _fuegoFrames.Length > 0)
                {
                    _fuegoTimer += dt;
                    if (_fuegoTimer >= FUEGO_INTERVALO)
                    {
                        _fuegoTimer = 0f;
                        _fuegoIdx = (_fuegoIdx + 1) % _fuegoFrames.Length;
                    }
                }
            }

            UpdateAnimation(dt);
        }

        public override void Render()
        {
            base.Render();

            // El sprite del fuego solo se ve cuando el haz esta encendido
            if (!_hazActivo) return;

            (int bx, int by, int bw, int bh) = AreaHaz();

            if (_fuegoFrames != null && _fuegoFrames.Length > 0)
            {
                // El sprite del fuego es horizontal (apunta a la derecha). Se dibuja
                // SIEMPRE en su orientacion natural (3 tiles de largo x 1 de alto)
                // centrado en el area y se ROTA segun la direccion, asi no se
                // deforma cuando el haz va hacia arriba o abajo.
                int cx    = bx + bw / 2;
                int cy    = by + bh / 2;
                int largo = 3 * Map.TILE;
                int alto  = Map.TILE;
                SDL.SDL_Rect dest = new SDL.SDL_Rect
                {
                    x = cx - largo / 2,
                    y = cy - alto / 2,
                    w = largo,
                    h = alto
                };

                Image frame = _fuegoFrames[_fuegoIdx % _fuegoFrames.Length];
                SDL.SDL_RenderCopyEx(Engine.Engine.renderer, frame.Pointer,
                    IntPtr.Zero, ref dest, AnguloHaz(), IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
            }
            else
            {
                // Respaldo si no hay sprite: rectangulo naranja con el area real
                SDL.SDL_Rect dest = new SDL.SDL_Rect { x = bx, y = by, w = bw, h = bh };
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 255, 120, 0, 255);
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref dest);
            }
        }

        // Angulo (horario) para rotar el sprite del fuego. El sprite viene al reves
        // (la punta apunta hacia la torre), asi que se rota 180 respecto del sentido
        // del haz para que la llama salga hacia afuera.
        private double AnguloHaz()
        {
            switch (_dir)
            {
                case Direccion.Derecha:   return 180;
                case Direccion.Abajo:     return 270;
                case Direccion.Izquierda: return 0;
                default:                  return 90;    // Arriba
            }
        }

        // Rectangulo que cubre el haz de 3 tiles segun la direccion
        private (int, int, int, int) AreaHaz()
        {
            int largo = 3 * Map.TILE;
            switch (_dir)
            {
                case Direccion.Derecha:   return (X + Map.TILE, Y, largo, Map.TILE);
                case Direccion.Izquierda: return (X - largo, Y, largo, Map.TILE);
                case Direccion.Arriba:    return (X, Y - largo, Map.TILE, largo);
                default:                  return (X, Y + Map.TILE, Map.TILE, largo); // Abajo
            }
        }

        private void DanarEnArea(List<Enemy> enemies)
        {
            (int bx, int by, int bw, int bh) = AreaHaz();
            foreach (var e in enemies)
            {
                if (!e.IsAlive) continue;
                if (e.Aereo) continue;   // el mago no alcanza a los voladores
                if (e.CollidesWith(bx, by, bw, bh))
                {
                    e.TakeDamage((int)Dano);
                    if (_slowFactor < 1f)
                        e.AplicarRalentizacion(_slowFactor, GameManager.SLOW_DURACION);
                }
            }
        }
    }
}
