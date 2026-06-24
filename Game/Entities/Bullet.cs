using ProyectoSDL2.Engine;
using ProyectoSDL2.Game;
using ProyectoSDL2.Game.Pooling;
using SDL2;
using System;

namespace ProyectoSDL2.Game.Entities
{
    // Proyectil de la torre arquera (homing: recalcula la direccion al objetivo
    // cada frame). Es POOLEABLE: el GestorDeBalas lo obtiene y lo devuelve a un
    // Pool<Bullet> en lugar de crearlo/destruirlo en cada disparo.
    public class Bullet : IPoolable
    {
        public Vector2 Position;
        public bool IsAlive { get; private set; }

        private Enemy   _target;
        private int     _damage;
        private Image   _sprite;
        private Vector2 _dir;

        private const float Speed     = 700f;
        private const int   HitRadius = 20;
        private const int   W         = 7;
        private const int   H         = 28;

        // Constructor vacio requerido por el Pool<Bullet> (restriccion new()).
        public Bullet() { }

        // Inicializa/reinicializa la bala con los datos del disparo. Se llama
        // justo despues de obtenerla del pool, reemplazando al viejo constructor.
        public void Configure(Vector2 startPos, Enemy target, int damage, Image sprite)
        {
            Position = startPos;
            _target  = target;
            _damage  = damage;
            _sprite  = sprite;

            float dx = target.Position.X - startPos.X;
            float dy = target.Position.Y - startPos.Y;
            _dir = new Vector2(dx, dy).Normalized();
        }

        // ── IPoolable ─────────────────────────────────────────────────────────
        public void AlObtener()  { IsAlive = true; }
        public void AlDevolver() { IsAlive = false; _target = null; }

        public void Update(float dt)
        {
            if (!IsAlive) return;

            if (_target != null && _target.IsAlive)
            {
                float dx = _target.Position.X - Position.X;
                float dy = _target.Position.Y - Position.Y;
                _dir = new Vector2(dx, dy).Normalized();
            }

            Position = Position + _dir * Speed * dt;

            if (_target != null && _target.IsAlive && Vector2.Distance(Position, _target.Position) < HitRadius)
            {
                _target.TakeDamage(_damage);
                IsAlive = false;
            }

            if (Position.X < -100 || Position.X > 1200 || Position.Y < -100 || Position.Y > 900)
                IsAlive = false;
        }

        public void Render()
        {
            if (!IsAlive) return;

            double angle = Math.Atan2(_dir.Y, _dir.X) * (180.0 / Math.PI) + 90.0;

            SDL.SDL_Rect dest = new SDL.SDL_Rect
            {
                x = (int)Position.X - W / 2,
                y = (int)Position.Y - H / 2,
                w = W,
                h = H
            };

            SDL.SDL_Point center = new SDL.SDL_Point { x = W / 2, y = H / 2 };

            SDL.SDL_RenderCopyEx(
                Engine.Engine.renderer,
                _sprite.Pointer,
                IntPtr.Zero,
                ref dest,
                angle,
                ref center,
                SDL.SDL_RendererFlip.SDL_FLIP_NONE
            );
        }
    }
}
