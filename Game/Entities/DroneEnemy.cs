using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Enemigo veloz: +70% velocidad, -40% vida, vale más monedas
    public class DroneEnemy : Enemy
    {
        public DroneEnemy(List<Vector2> waypoints, Image sheet)
            : base(waypoints, sheet)
        {
            Health         = 60;
            Velocidad      = 102f;   // 60 * 1.7
            MonedasAlMorir = 15;
            totalFrames    = 4;
            intervalFrame  = 0.07f;  // animación más rápida
        }

        // Sobreescribe Update para el movimiento más veloz (polimorfismo)
        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
