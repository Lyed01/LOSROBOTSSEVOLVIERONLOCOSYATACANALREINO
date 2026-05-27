using ProyectoSDL2.Engine;

namespace ProyectoSDL2.Game.Entities
{
    public class ArcherTower : Tower
    {
        public ArcherTower(int x, int y, Image sheet, Image bulletImg)
            : base(x, y, sheet, bulletImg)
        {
            Rango         = 200f;
            Dano          = 20f;
            cadencia      = 1.5f;
            totalFrames   = 2;
            intervalFrame = 0.15f;
        }

        public override void Attack() { }
    }
}
