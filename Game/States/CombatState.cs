using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using ProyectoSDL2.Game.Factories;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;
using System.Collections.Generic;
using SDL2;
namespace ProyectoSDL2.Game.States
{
    public class CombatState : IGameState
    {
        private Image _fondo;
        private StateManager _sm;
        private Map          _map;
        private List<Tower>  _torres;
        private List<Enemy>  _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();
        private Font         _fontHud;
        private Image[] _imgSoldier;
        private Image[] _imgDrone;
        private Castle _castle; 
        // Spawn
        private float _spawnTimer    = 0f;
        private float _spawnInterval = 2.5f;

        private int   _enemigosSpawneados = 0;
        private int   _enemigosTotal      = 20;
        private int   _enemigosEliminados = 0;

        public CombatState(StateManager sm, Map map, List<Tower> torres)
        {
            _sm     = sm;
            _map    = map;
            _torres = torres;
        }

        public void Enter()
        {
            _fondo = Engine.Engine.LoadImage("Assets/fondo.png");
            _fontHud = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 22);
            _castle = new Castle(15 * 64, 0, Engine.Engine.LoadImage("Assets/sprites/castle.png"));
            _imgSoldier = new Image[]
            {
                Engine.Engine.LoadImage("Assets/TankEnemy/0.png"),
                Engine.Engine.LoadImage("Assets/TankEnemy/1.png"),
                Engine.Engine.LoadImage("Assets/TankEnemy/2.png"),
                Engine.Engine.LoadImage("Assets/TankEnemy/3.png"),
            };
            _imgDrone = new Image[]
            {
                Engine.Engine.LoadImage("Assets/FastEnemy/0.png"),
                Engine.Engine.LoadImage("Assets/FastEnemy/1.png"),
                Engine.Engine.LoadImage("Assets/FastEnemy/2.png"),
                Engine.Engine.LoadImage("Assets/FastEnemy/3.png"),
            };


            GameManager.Instance.SetState(GameState.Combat);

            // Suscribirse a eventos del GameManager
            GameManager.Instance.OnEnemyDied  += OnEnemyDied;
            GameManager.Instance.OnCastleHit  += OnCastleHit;
        }

        public void Update(float dt)
        {
            // Spawn de enemigos
            if (_enemigosSpawneados < _enemigosTotal)
            {
                _spawnTimer += dt;
                if (_spawnTimer >= _spawnInterval)
                {
                    _spawnTimer = 0f;
                    SpawnEnemy();
                }
            }

            // Actualizar enemigos
            foreach (var e in _enemies)
                e.Update(dt);

            // Actualizar torres (buscan, disparan y atacan enemigos)
            foreach (var t in _torres)
                t.Update(dt, _enemies, _bullets);

            // Actualizar balas
            foreach (var b in _bullets)
                b.Update(dt);
            _bullets.RemoveAll(b => !b.IsAlive);

            // Limpiar muertos
            _enemies.RemoveAll(e => !e.IsAlive);

            // Verificar fin de oleada
            if (_enemigosSpawneados >= _enemigosTotal && _enemies.Count == 0)
                GameManager.Instance.WaveComplete();

            // Chequear derrota/victoria por estado
            if (GameManager.Instance.Estado == GameState.Defeat)
                _sm.ChangeState(new DefeatState(_sm));
            else if (GameManager.Instance.Estado == GameState.Victory)
                _sm.ChangeState(new VictoryState(_sm));
            else if (GameManager.Instance.Estado == GameState.SkillTree)
                _sm.ChangeState(new PlanningState(_sm, _map, _torres)); // por ahora salta directo a planning
        }

        public void Render()
        {
            Engine.Engine.Clear();

            SDL.SDL_Rect destFondo = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 576 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref destFondo);

            foreach (var t in _torres)  t.Render();
            foreach (var e in _enemies) e.Render();
            foreach (var b in _bullets) b.Render();

            // HUD
            Engine.Engine.DrawText($"Monedas: {GameManager.Instance.Monedas}",  10,  10, 255, 215,   0, _fontHud);
            Engine.Engine.DrawText($"Castillo: {GameManager.Instance.Hits}/3",  10,  40, 255, 100, 100, _fontHud);
            Engine.Engine.DrawText($"Oleada: {GameManager.Instance.OleadaActual}/{GameManager.Instance.TotalOleadas}", 10, 70, 255, 255, 255, _fontHud);
            Engine.Engine.DrawText($"Enemigos: {_enemigosEliminados}/{_enemigosTotal}", 10, 100, 200, 200, 200, _fontHud);
            _castle.Render();
            Engine.Engine.Show();
        }

        public void Exit()
        {
            GameManager.Instance.OnEnemyDied -= OnEnemyDied;
            GameManager.Instance.OnCastleHit -= OnCastleHit;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SpawnEnemy()
        {
            // Alternar entre soldado y drone cada 3 spawns
            EnemyType tipo   = (_enemigosSpawneados % 3 == 2) ? EnemyType.Drone : EnemyType.Soldier;
            Image[] frames = tipo == EnemyType.Drone ? _imgDrone : _imgSoldier;
            Enemy enemy = EnemyFactory.Create(tipo, _map.Waypoints, frames);
            _enemies.Add(enemy);
            _enemigosSpawneados++;
        }

        private void OnEnemyDied(int monedas)
        {
            _enemigosEliminados++;
        }

        private void OnCastleHit()
        {
            Engine.Engine.Debug($"¡El castillo recibió un golpe! Hits restantes: {GameManager.Instance.Hits}");
        }
    }
}
