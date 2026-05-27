using ProyectoSDL2.Engine;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Torre hachera: rango corto, daño alto en área, cadencia baja
    public class AxeTower : Tower
    {
        private float _areaRango = 60f;

        public AxeTower(int x, int y, Image sheet)
            : base(x, y, sheet)
        {
            Rango    = 80f;
            Dano     = 50f;
            cadencia = 0.5f;   // 1 ataque cada 2 segundos
            totalFrames   = 2;
            intervalFrame = 0.2f;
        }

        public override void Attack() { }

        // Override Update para aplicar daño de área instantáneo (sin proyectil)
        public override void Update(float dt, List<Enemy> enemies, List<Bullet> bullets)
        {
            timerAtaque += dt;
            atacando     = false;

            if (timerAtaque >= 1f / cadencia)
            {
                bool hayEnemigoCerca = false;

                foreach (var e in enemies)
                {
                    if (!e.IsAlive) continue;
                    float dx   = e.Position.X - X;
                    float dy   = e.Position.Y - Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist <= Rango)
                        hayEnemigoCerca = true;

                    if (dist <= _areaRango)
                        e.TakeDamage((int)Dano);
                }

                if (hayEnemigoCerca)
                {
                    atacando    = true;
                    timerAtaque = 0f;
                    Attack();
                }
            }

            UpdateAnimation(dt);
        }
    }
}
