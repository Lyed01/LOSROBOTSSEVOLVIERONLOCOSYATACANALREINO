using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    public class DroneEnemy : Enemy
    {
        public DroneEnemy(List<Vector2> waypoints, Image[] frames)
            : base(waypoints, frames)
        {
            Health         = 100;
            Velocidad      = 102f;
            MonedasAlMorir = 3;
            totalFrames    = 4;
            intervalFrame  = 0.07f;
        }
    }
}
