using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Interfaces;
using SDL2;
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
            SDL.SDL_Rect dest = new SDL.SDL_Rect { x = X, y = Y, w = 192, h = 192 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _sprite.Pointer, IntPtr.Zero, ref dest);
        }
    }
}
