using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Factories
{
    public enum EnemyType { Soldier, Drone }

    public static class EnemyFactory
    {
        public static Enemy Create(EnemyType type, List<Vector2> waypoints, Image sheet)
        {
            return type switch
            {
                EnemyType.Soldier => new SoldierEnemy(waypoints, sheet),
                EnemyType.Drone   => new DroneEnemy(waypoints, sheet),
                _ => throw new ArgumentException($"Tipo de enemigo desconocido: {type}")
            };
        }
    }
}
