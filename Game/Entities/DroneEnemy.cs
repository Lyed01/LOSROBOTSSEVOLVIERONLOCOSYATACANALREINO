using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    public class DroneEnemy : Enemy
    {
        public DroneEnemy(List<Vector2> waypoints, Image[] frames)
            : base(waypoints, frames)
        {
            Health           = 100;
            Velocidad        = 102f;
            Aereo            = true;    // vuela: solo el arquero puede dañarlo
            MonedasAlMorir   = 3;
            CristalChance    = 0.25f;   // 25% de soltar cristal
            CristalesAlMorir = 1;
            totalFrames      = 4;
            intervalFrame    = 0.07f;
        }
    }
}
