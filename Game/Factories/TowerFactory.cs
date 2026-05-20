using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using System;

namespace ProyectoSDL2.Game.Factories
{
    public enum TowerType { Archer, Axe }

    public static class TowerFactory
    {
        public static Tower Create(TowerType type, int x, int y, Image sheet)
        {
            return type switch
            {
                TowerType.Archer => new ArcherTower(x, y, sheet),
                TowerType.Axe    => new AxeTower(x, y, sheet),
                _ => throw new ArgumentException($"Tipo de torre desconocido: {type}")
            };
        }
    }
}
