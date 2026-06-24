using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Managers;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Boss: muchisima vida. Aparece al final de cada nivel (ronda 3).
    // Si llega al castillo se pierde la partida directamente.
    public class BossEnemy : Enemy
    {
        public BossEnemy(List<Vector2> waypoints, Image[] frames)
            : base(waypoints, frames)
        {
            // Vida base a la mitad: en el nivel 1 el boss es accesible y crece
            // con los niveles via MultiplicadorVida() (aplicado en EnemyFactory).
            Health           = 1500;
            Velocidad        = 27f;   // 10% mas lento
            MonedasAlMorir   = 50;
            CristalChance    = 1.0f;   // siempre suelta cristales
            CristalesAlMorir = 5;
            totalFrames      = 6;     // 6 frames de animacion
            intervalFrame    = 0.2f;
            Width            = 72;
            Height           = 72;
        }

        protected override void LlegarAlCastillo()
        {
            GameManager.Instance.PerderPartida();
        }
    }
}
