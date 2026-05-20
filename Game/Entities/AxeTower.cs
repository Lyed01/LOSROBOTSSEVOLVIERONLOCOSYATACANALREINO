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

        public override void Attack()
        {
            Engine.Engine.Debug("AxeTower: ¡golpe de área!");
        }

        // Sobreescribe ApplyDamage para dañar a TODOS los enemigos en el área
        protected override void ApplyDamage(Enemy primaryTarget)
        {
            // El daño se aplica al objetivo principal y a los cercanos.
            // El Update de Tower llama a Attack() + ApplyDamage(primerEnemigo),
            // pero necesitamos la lista completa → se maneja en Update override.
        }

        // Override Update para aplicar daño en área
        public override void Update(float dt, List<Enemy> enemies)
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
