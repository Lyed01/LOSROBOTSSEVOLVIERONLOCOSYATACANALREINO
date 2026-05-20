using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Factories
{
    public enum EnemyType { Soldier, Drone }

    public static class EnemyFactory
    {
        public static Enemy Create(EnemyType type, List<Vector2> waypoints, Image[] frames)
        {
            return type switch
            {
                EnemyType.Soldier => new SoldierEnemy(waypoints, frames),
                EnemyType.Drone => new DroneEnemy(waypoints, frames),
                _ => throw new ArgumentException($"Tipo de enemigo desconocido: {type}")
            };
        }
    }
}
