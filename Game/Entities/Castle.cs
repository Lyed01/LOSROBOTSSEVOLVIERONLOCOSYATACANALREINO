using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;

namespace ProyectoSDL2.Game.Entities
{
    public class Castle : IDamageable
    {
        public int  X      { get; }
        public int  Y      { get; }
        public int  Health { get; private set; } = 3;
        public bool IsAlive => Health > 0;

        private Image _sprite;

        public Castle(int x, int y, Image sprite)
        {
            X = x; Y = y;
            _sprite = sprite;
        }

        public void TakeDamage(int amount)
        {
            // El daño al castillo se gestiona vía GameManager.TakeCastleHit()
            // para que dispare el evento correspondiente
        }

        public void Render()
        {
            Engine.Engine.Draw(_sprite, X, Y);
        }
    }
}
