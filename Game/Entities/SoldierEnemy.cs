using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Enemigo estándar: velocidad y vida base
    public class SoldierEnemy : Enemy
    {
        public SoldierEnemy(List<Vector2> waypoints, Image sheet)
            : base(waypoints, sheet)
        {
            Health         = 100;
            Velocidad      = 60f;
            MonedasAlMorir = 10;
            totalFrames    = 4;
            intervalFrame  = 0.12f;
        }
    }
}
