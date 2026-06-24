using ProyectoSDL2.Engine;
using ProyectoSDL2.Game;
using ProyectoSDL2.Game.Pooling;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Entities
{
    // Administra el ciclo de vida de las balas en combate usando un Pool<Bullet>
    // generico. Encapsula la reserva de balas (pool) y la lista de balas activas,
    // de modo que las torres y el CombatState no manejan esos detalles (SRP).
    //
    //   Disparar() -> obtiene una bala del pool, la configura y la activa
    //   Update()   -> avanza las balas y devuelve al pool las que ya murieron
    //   Render()   -> dibuja las balas activas
    public class GestorDeBalas
    {
        private readonly Pool<Bullet> _pool;
        private readonly List<Bullet> _activas = new List<Bullet>();

        public GestorDeBalas(int precarga = 20)
        {
            _pool = new Pool<Bullet>(precarga);
        }

        // Lanza una bala hacia un objetivo reutilizando una instancia del pool.
        public void Disparar(Vector2 origen, Enemy objetivo, int dano, Image sprite)
        {
            Bullet b = _pool.Obtener();
            b.Configure(origen, objetivo, dano, sprite);
            _activas.Add(b);
        }

        public void Update(float dt)
        {
            for (int i = _activas.Count - 1; i >= 0; i--)
            {
                _activas[i].Update(dt);

                // La bala que dejo de estar viva vuelve al pool para reciclarse.
                if (!_activas[i].IsAlive)
                {
                    _pool.Devolver(_activas[i]);
                    _activas.RemoveAt(i);
                }
            }
        }

        public void Render()
        {
            foreach (var b in _activas)
                b.Render();
        }
    }
}
