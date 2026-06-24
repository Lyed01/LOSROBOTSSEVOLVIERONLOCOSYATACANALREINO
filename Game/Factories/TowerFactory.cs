using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using ProyectoSDL2.Game.Managers;
using System;

namespace ProyectoSDL2.Game.Factories
{
    public enum TowerType { Archer, Axe, Mage }

    public static class TowerFactory
    {
        public static Tower Create(TowerType type, int x, int y, Image sheet,
            Image bulletSheet = null, Direccion dir = Direccion.Derecha,
            Image[] fuego = null, Image[] framesAtaque = null)
        {
            Tower torre = type switch
            {
                TowerType.Archer => new ArcherTower(x, y, sheet, bulletSheet),
                TowerType.Axe    => new AxeTower(x, y, sheet, framesAtaque),
                TowerType.Mage   => new MageTower(x, y, sheet, dir, fuego),
                _ => throw new ArgumentException($"Tipo de torre desconocido: {type}")
            };

            // Aplicar las mejoras de ESTE tipo de torre desbloqueadas en el arbol
            GameManager gm = GameManager.Instance;
            DatosTorre datos = type switch
            {
                TowerType.Axe  => gm.Hachero,
                TowerType.Mage => gm.Mago,
                _              => gm.Arquero
            };
            torre.AplicarMejora(
                datos.NivelDano     * GameManager.BONO_DANO,
                datos.NivelRango    * GameManager.BONO_RANGO,
                datos.NivelCadencia * GameManager.BONO_CADENCIA);

            // Solo el mago usa la ralentizacion del arbol
            if (torre is MageTower mago)
                mago.ConfigurarRalentizacion(gm.FactorSlowMago());

            // Mejora definitiva de la rama (late game)
            if (datos.MejoraEspecial)
            {
                switch (type)
                {
                    case TowerType.Archer: torre.DisparosPorAtaque = 2;                  break;
                    case TowerType.Axe:    ((AxeTower)torre).ActivarOndaExpansiva();      break;
                    case TowerType.Mage:   ((MageTower)torre).ActivarHazPermanente();     break;
                }
            }

            return torre;
        }
    }
}
