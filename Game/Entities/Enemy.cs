using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;
using System;
using System.Collections.Generic;
using SDL2;

namespace ProyectoSDL2.Game.Entities
{
    public abstract class Enemy : IDamageable, IAnimatable
    {
        // ── Posición y tamaño ──────────────────────────────────────────────────
        public Vector2 Position;
        public int     Width  = 48;
        public int     Height = 48;

        // ── IDamageable ────────────────────────────────────────────────────────
        public int  Health    { get; protected set; }
        public int  MaxHealth { get; protected set; }
        public bool IsAlive   { get; protected set; } = true;

        // ── Stats ──────────────────────────────────────────────────────────────
        public float Velocidad        { get; protected set; } = 60f;
        public int   MonedasAlMorir   { get; protected set; } = 10;

        // Enemigo aereo: el hachero (golpe a ras del suelo) y el mago (haz) no lo
        // alcanzan; solo lo daña el arquero. Lo activan las subclases voladoras.
        public bool  Aereo            { get; protected set; } = false;
        public float CristalChance    { get; protected set; } = 0f;   // 0 a 1
        public int   CristalesAlMorir { get; protected set; } = 1;

        // Random compartido para los drops
        private static Random _random = new Random();

        // ── Ralentizacion (mago) ───────────────────────────────────────────────
        // Multiplicador temporal de velocidad (1 = sin efecto). Se refresca con
        // cada tick del haz del mago y se descuenta en Update.
        private float _slowFactor = 1f;
        private float _slowTimer  = 0f;

        // Aplica/renueva una ralentizacion. factor < 1 frena; duracion en segundos.
        public void AplicarRalentizacion(float factor, float duracion)
        {
            _slowFactor = factor;
            _slowTimer  = duracion;
        }

        // ── Waypoints ──────────────────────────────────────────────────────────
        protected List<Vector2> waypoints;
        private   int           waypointIndex = 0;
        private   const float   WAYPOINT_THRESHOLD = 6f;

        // ── Animación ──────────────────────────────────────────────────────────
        protected Image[]  spriteSheet;
        protected int    frameActual   = 0;
        protected float  timerFrame    = 0f;
        protected float  intervalFrame = 0.12f;  // segundos por frame
        protected int    totalFrames   = 4;
        protected int    frameW        = 48;
        protected int    frameH        = 48;

        // ── Evento ────────────────────────────────────────────────────────────
        public event Action OnDied;

        protected Enemy(List<Vector2> waypoints, Image[] frames)
        {
            this.waypoints = waypoints;
            this.spriteSheet = frames;
            if (waypoints.Count > 0)
                Position = waypoints[0];
        }

        // Multiplica la vida del enemigo segun la dificultad de la ronda.
        // Tambien fija la vida maxima para la barra de vida.
        public void EscalarVida(float multiplicador)
        {
            Health    = (int)(Health * multiplicador);
            MaxHealth = Health;
        }

        // Multiplica la velocidad del enemigo segun la dificultad de la ronda.
        public void EscalarVelocidad(float multiplicador)
        {
            Velocidad *= multiplicador;
        }

        // ── IDamageable ────────────────────────────────────────────────────────
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health <= 0)
            {
                Health  = 0;
                IsAlive = false;
                OnDied?.Invoke();
                GameManager.Instance.EnemyDied(MonedasAlMorir);

                // Drop de cristales segun la chance del enemigo
                if (_random.NextDouble() < CristalChance)
                    GameManager.Instance.AddCristales(CristalesAlMorir);
            }
        }

        // ── IAnimatable ───────────────────────────────────────────────────────
        public virtual void UpdateAnimation(float dt)
        {
            timerFrame += dt;
            if (timerFrame >= intervalFrame)
            {
                timerFrame = 0f;
                frameActual = (frameActual + 1) % totalFrames;
            }
        }

        // ── Movimiento por waypoints ───────────────────────────────────────────
        public virtual void Update(float dt)
        {
            if (!IsAlive || waypointIndex >= waypoints.Count) return;

            // Descontar la ralentizacion activa
            if (_slowTimer > 0f)
            {
                _slowTimer -= dt;
                if (_slowTimer <= 0f) _slowFactor = 1f;
            }

            Vector2 target = waypoints[waypointIndex];
            Vector2 dir    = (target - Position).Normalized();
            Position = Position + dir * (Velocidad * _slowFactor) * dt;

            if (Vector2.Distance(Position, target) < WAYPOINT_THRESHOLD)
            {
                waypointIndex++;

                // Llegó al castillo (último waypoint)
                if (waypointIndex >= waypoints.Count)
                {
                    IsAlive = false;
                    LlegarAlCastillo();
                }
            }

            UpdateAnimation(dt);
        }

        // Que pasa cuando el enemigo llega al castillo. Por defecto resta un golpe;
        // el boss lo sobreescribe para causar derrota directa.
        protected virtual void LlegarAlCastillo()
        {
            GameManager.Instance.TakeCastleHit();
        }

        // Escala de pixel comun a TODOS los enemigos. Cada sprite se dibuja segun
        // su resolucion nativa por este mismo factor, asi el "pixel" mide igual en
        // todos (los de mas resolucion se ven naturalmente mas grandes) y no se
        // distorsiona el aspecto al no forzar un cuadrado fijo.
        protected const float ESCALA_PX = 2.2f;

        // ── Render ────────────────────────────────────────────────────────────
        public virtual void Render()
        {
            if (!IsAlive) return;

            Image f = spriteSheet[frameActual];
            int w  = (int)(f.Width  * ESCALA_PX);
            int h  = (int)(f.Height * ESCALA_PX);
            int cx = (int)Position.X + 32;   // centro de la antigua caja 64x64
            int cy = (int)Position.Y + 32;
            int rx = cx - w / 2;
            int ry = cy - h / 2;

            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = rx, y = ry, w = w, h = h };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, f.Pointer, IntPtr.Zero, ref dest);

            // Barra de vida centrada arriba del sprite
            if (MaxHealth > 0)
            {
                int barW = 40;
                int barH = 5;
                int bx   = cx - barW / 2;
                int by   = ry - 8;

                // Fondo rojo (vida perdida)
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 180, 40, 40, 255);
                SDL.SDL_Rect fondo = new SDL.SDL_Rect { x = bx, y = by, w = barW, h = barH };
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref fondo);

                // Vida actual (verde)
                int vidaW = (int)(barW * (Health / (float)MaxHealth));
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 60, 200, 80, 255);
                SDL.SDL_Rect vida = new SDL.SDL_Rect { x = bx, y = by, w = vidaW, h = barH };
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref vida);
            }
        }

        // ── Colisión AABB ─────────────────────────────────────────────────────
        public bool CollidesWith(int ox, int oy, int ow, int oh)
        {
            int x = (int)Position.X;
            int y = (int)Position.Y;
            return x < ox + ow && x + Width > ox &&
                   y < oy + oh && y + Height > oy;
        }
    }
}
