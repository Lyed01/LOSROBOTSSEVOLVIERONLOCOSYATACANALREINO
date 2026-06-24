using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using ProyectoSDL2.Game.Managers;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Factories
{
    public enum EnemyType { Soldier, Drone, MegaTank, Boss }

    public static class EnemyFactory
    {
        public static Enemy Create(EnemyType type, List<Vector2> waypoints, Image[] frames)
        {
            Enemy enemigo = type switch
            {
                EnemyType.Soldier  => new SoldierEnemy(waypoints, frames),
                EnemyType.Drone    => new DroneEnemy(waypoints, frames),
                EnemyType.MegaTank => new MegaTankEnemy(waypoints, frames),
                EnemyType.Boss     => new BossEnemy(waypoints, frames),
                _ => throw new ArgumentException($"Tipo de enemigo desconocido: {type}")
            };

            // Escalar la vida y la velocidad segun la dificultad de la ronda actual
            enemigo.EscalarVida(GameManager.Instance.MultiplicadorVida());
            enemigo.EscalarVelocidad(GameManager.Instance.MultiplicadorVelocidad());

            return enemigo;
        }
    }
}
