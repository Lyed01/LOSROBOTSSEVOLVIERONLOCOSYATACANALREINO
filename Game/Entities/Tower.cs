using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game;
using System;
using System.Collections.Generic;
using SDL2;

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
        protected float cadencia;
        protected float timerAtaque = 0f;

        // ── Animación ─────────────────────────────────────────────────────────
        protected Image  spriteSheet;
        protected Image  _bulletImg;
        protected int    frameActual   = 0;
        protected float  timerFrame    = 0f;
        protected float  intervalFrame = 0.15f;
        protected int    totalFrames   = 2;
        protected int    frameW        = 64;
        protected int    frameH        = 64;
        protected bool   atacando      = false;

        protected Tower(int x, int y, Image sheet, Image bulletImg = null)
        {
            X = x; Y = y;
            spriteSheet = sheet;
            _bulletImg  = bulletImg;
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
        public virtual void Update(float dt, List<Enemy> enemies, List<Bullet> bullets)
        {
            timerAtaque += dt;
            atacando     = false;

            if (timerAtaque >= 1f / cadencia)
            {
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
                        Shoot(e, bullets);
                        break;
                    }
                }
            }

            UpdateAnimation(dt);
        }

        // Dispara una bala hacia el objetivo; las subclases pueden sobreescribir
        protected virtual void Shoot(Enemy target, List<Bullet> bullets)
        {
            if (_bulletImg == null) return;
            int cx = X + Map.TILE / 2;
            int cy = Y + Map.TILE / 2 - 128;
            bullets.Add(new Bullet(new Vector2(cx, cy), target, (int)Dano, _bulletImg));
        }

        // ── Render ────────────────────────────────────────────────────────────
        public virtual void Render()
        {
            int renderW = 128, renderH = 128;
            int renderX = X + (Map.TILE / 2) - (renderW / 2);
            int renderY = Y + (Map.TILE / 2) - renderH;
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = renderX, y = renderY, w = renderW, h = renderH };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, spriteSheet.Pointer, IntPtr.Zero, ref dest);
        }
    }
}
