using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    public abstract class Enemy : IDamageable, IAnimatable
    {
        // ── Posición y tamaño ──────────────────────────────────────────────────
        public Vector2 Position;
        public int     Width  = 48;
        public int     Height = 48;

        // ── IDamageable ────────────────────────────────────────────────────────
        public int  Health  { get; protected set; }
        public bool IsAlive { get; protected set; } = true;

        // ── Stats ──────────────────────────────────────────────────────────────
        public float Velocidad      { get; protected set; } = 60f;
        public int   MonedasAlMorir { get; protected set; } = 10;

        // ── Waypoints ──────────────────────────────────────────────────────────
        protected List<Vector2> waypoints;
        private   int           waypointIndex = 0;
        private   const float   WAYPOINT_THRESHOLD = 6f;

        // ── Animación ──────────────────────────────────────────────────────────
        protected Image  spriteSheet;
        protected int    frameActual   = 0;
        protected float  timerFrame    = 0f;
        protected float  intervalFrame = 0.12f;  // segundos por frame
        protected int    totalFrames   = 4;
        protected int    frameW        = 48;
        protected int    frameH        = 48;

        // ── Evento ────────────────────────────────────────────────────────────
        public event Action OnDied;

        protected Enemy(List<Vector2> waypoints, Image sheet)
        {
            this.waypoints  = waypoints;
            this.spriteSheet = sheet;
            if (waypoints.Count > 0)
                Position = waypoints[0];
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

            Vector2 target = waypoints[waypointIndex];
            Vector2 dir    = (target - Position).Normalized();
            Position = Position + dir * Velocidad * dt;

            if (Vector2.Distance(Position, target) < WAYPOINT_THRESHOLD)
            {
                waypointIndex++;

                // Llegó al castillo (último waypoint)
                if (waypointIndex >= waypoints.Count)
                {
                    IsAlive = false;
                    GameManager.Instance.TakeCastleHit();
                }
            }

            UpdateAnimation(dt);
        }

        // ── Render ────────────────────────────────────────────────────────────
        public virtual void Render()
        {
            if (!IsAlive) return;
            // Se usa SDL_RenderCopy con sourceRect del frame actual
            int srcX = frameActual * frameW;
            Engine.Engine.Draw(spriteSheet, (int)Position.X, (int)Position.Y);
            // TODO: cuando tengan spritesheet real, reemplazar con el overload de src rect
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
