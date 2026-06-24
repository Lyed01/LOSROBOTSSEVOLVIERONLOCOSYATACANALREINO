using ProyectoSDL2.Engine;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Mega tanque: mucha vida y lento. Suelta buenos recursos.
    public class MegaTankEnemy : Enemy
    {
        public MegaTankEnemy(List<Vector2> waypoints, Image[] frames)
            : base(waypoints, frames)
        {
            Health           = 600;
            Velocidad        = 40f;
            MonedasAlMorir   = 8;
            CristalChance    = 0.40f;
            CristalesAlMorir = 2;
            totalFrames      = 6;     // 6 frames de animacion
            intervalFrame    = 0.14f;
            Width            = 56;
            Height           = 56;
        }

        // Se dibuja un poco mas grande por ser un tanque
        public override void Render()
        {
            base.Render();
        }
    }
}
