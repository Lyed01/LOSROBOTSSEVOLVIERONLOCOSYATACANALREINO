using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    public abstract class Tower : IAttacker, IAnimatable
    {
        // ── Posición ──────────────────────────────────────────────────────────
        public int X { get; set; }
        public int Y { get; set; }
        public int Width  = 64;
        public int Height = 64;

        // ── IAttacker ─────────────────────────────────────────────────────────
        public float Rango { get; protected set; }
        public float Dano  { get; protected set; }

        // ── Cadencia ──────────────────────────────────────────────────────────
        protected float cadencia;      // ataques por segundo
        protected float timerAtaque = 0f;

        // ── Animación ─────────────────────────────────────────────────────────
        protected Image  spriteSheet;
        protected int    frameActual   = 0;
        protected float  timerFrame    = 0f;
        protected float  intervalFrame = 0.15f;
        protected int    totalFrames   = 2;
        protected int    frameW        = 64;
        protected int    frameH        = 64;
        protected bool   atacando      = false;

        protected Tower(int x, int y, Image sheet)
        {
            X = x; Y = y;
            spriteSheet = sheet;
        }

        // ── IAttacker ─────────────────────────────────────────────────────────
        public abstract void Attack();

        // ── IAnimatable ───────────────────────────────────────────────────────
        public virtual void UpdateAnimation(float dt)
        {
            timerFrame += dt;
            if (timerFrame >= intervalFrame)
            {
                timerFrame  = 0f;
                frameActual = (frameActual + 1) % totalFrames;
            }
        }

        // ── Update general ────────────────────────────────────────────────────
        public virtual void Update(float dt, List<Enemy> enemies)
        {
            timerAtaque += dt;
            atacando     = false;

            if (timerAtaque >= 1f / cadencia)
            {
                // Buscar enemigo en rango
                foreach (var e in enemies)
                {
                    if (!e.IsAlive) continue;
                    float dx   = e.Position.X - X;
                    float dy   = e.Position.Y - Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist <= Rango)
                    {
                        atacando    = true;
                        timerAtaque = 0f;
                        Attack();
                        ApplyDamage(e);
                        break;
                    }
                }
            }

            UpdateAnimation(dt);
        }

        protected virtual void ApplyDamage(Enemy target)
        {
            target.TakeDamage((int)Dano);
        }

        // ── Render ────────────────────────────────────────────────────────────
        public virtual void Render()
        {
            Engine.Engine.Draw(spriteSheet, X, Y);
        }
    }
}
