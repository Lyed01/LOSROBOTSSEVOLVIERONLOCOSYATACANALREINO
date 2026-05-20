using ProyectoSDL2.Engine;

namespace ProyectoSDL2.Game.Entities
{
    // Torre de arquero: rango largo, daño bajo-medio, cadencia alta
    public class ArcherTower : Tower
    {
        public ArcherTower(int x, int y, Image sheet)
            : base(x, y, sheet)
        {
            Rango    = 200f;
            Dano     = 20f;
            cadencia = 1.5f;   // 1.5 ataques por segundo
            totalFrames   = 2;
            intervalFrame = 0.15f;
        }

        public override void Attack()
        {
            // Aquí se instanciaría un Projectile; por ahora el daño
            // se aplica directamente en Tower.ApplyDamage()
            Engine.Engine.Debug("ArcherTower: ¡disparo!");
        }
    }
}
