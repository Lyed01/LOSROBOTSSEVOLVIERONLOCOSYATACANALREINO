using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Enemigo estándar: velocidad y vida base
    public class SoldierEnemy : Enemy
    {
        public SoldierEnemy(List<Vector2> waypoints, Image[] frames)
            : base(waypoints, frames)
        {
            Health         = 200;
            Velocidad      = 60f;
            MonedasAlMorir = 2;
            totalFrames    = 4;
            intervalFrame  = 0.12f;
        }
    }
}
