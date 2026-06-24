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
        private Image _muro;   // decoracion del gap de abajo (puede ser null)
        private StateManager _sm;
        private Map          _map;
        private List<Tower>  _torres;
        private List<Enemy>  _enemies = new List<Enemy>();
        private GestorDeBalas _balas  = new GestorDeBalas();
        private Font         _fontHud;
        private Font         _fontTitulo;
        private Image[] _imgSoldier;
        private Image[] _imgDrone;
        private Image[] _imgMegaTank;
        private Image[] _imgBoss;
        private Castle _castle;
        private System.Random _rng = new System.Random();

        // Pausa
        private bool  _pausado = false;
        private float _pauseCooldown = 0f;

        // Velocidad del tiempo (x1 / x2 / x3) para acelerar la ronda
        private float _velTiempo   = 1f;
        private float _velCooldown = 0f;
        // Spawn por hordas: rafagas de tamaño variable con pausa entre ellas
        private float _spawnTimer    = 0f;
        private int   _hordaRestante = 0;                 // enemigos que faltan de la horda actual
        private const float DELAY_ENTRE_HORDAS = 2.2f;    // pausa antes de la proxima horda
        private const float DELAY_DENTRO_HORDA = 0.35f;   // separacion dentro de la misma horda

        // Fundido de salida al ganar la ronda (transicion suave hacia el arbol)
        private bool  _terminando = false;
        private float _fadeOut    = 0f;
        private const float FADE_VEL = 1.8f;

        private int   _enemigosSpawneados = 0;
        private int   _enemigosTotal      = 0;   // se calcula en Enter segun la ronda
        private int   _enemigosEliminados = 0;

        public CombatState(StateManager sm, Map map, List<Tower> torres)
        {
            _sm     = sm;
            _map    = map;
            _torres = torres;
        }

        public void Enter()
        {
            _fondo = Engine.Engine.LoadImage(_map.Fondo);
            _muro  = Engine.Engine.LoadImageSafe("Assets/Mapas/MuroAbajoDelMapa.png");
            _fontHud    = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 22);
            _fontTitulo = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 38);
            _castle = new Castle((int)_map.Castillo.X, (int)_map.Castillo.Y, Engine.Engine.LoadImage("Assets/sprites/castle.png"));
            _imgSoldier = new Image[]
            {
                Engine.Engine.LoadImage("Assets/sprites/regularEnemy/0.png"),
                Engine.Engine.LoadImage("Assets/sprites/regularEnemy/1.png"),
                Engine.Engine.LoadImage("Assets/sprites/regularEnemy/2.png"),
                Engine.Engine.LoadImage("Assets/sprites/regularEnemy/3.png"),
            };
            _imgDrone = new Image[]
            {
                Engine.Engine.LoadImage("Assets/sprites/FastEnemy/0.png"),
                Engine.Engine.LoadImage("Assets/sprites/FastEnemy/1.png"),
                Engine.Engine.LoadImage("Assets/sprites/FastEnemy/2.png"),
                Engine.Engine.LoadImage("Assets/sprites/FastEnemy/3.png"),
            };

            _imgMegaTank = new Image[]
            {
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo.png"),
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo2.png"),
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo3.png"),
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo4.png"),
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo5.png"),
                Engine.Engine.LoadImage("Assets/sprites/MegaTank/RobotGordo6.png"),
            };

            _imgBoss = new Image[]
            {
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss1.png"),
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss2.png"),
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss3.png"),
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss4.png"),
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss5.png"),
               Engine.Engine.LoadImage("Assets/sprites/Boss/Boss6.png"),
            };


            GameManager.Instance.SetState(GameState.Combat);

            // Cantidad de enemigos segun la dificultad de la ronda actual
            _enemigosTotal = GameManager.Instance.EnemigosDeLaRonda();

            // Suscribirse a eventos del GameManager
            GameManager.Instance.OnEnemyDied  += OnEnemyDied;
            GameManager.Instance.OnCastleHit  += OnCastleHit;

            // Musica: tema de jefe en la ronda final del nivel; si no, alterna pelea 1/2.
            bool rondaJefe = GameManager.Instance.RondaEnNivel == GameManager.RONDAS_POR_NIVEL;
            string pista = rondaJefe ? MusicaManager.JEFE
                         : (GameManager.Instance.RondaEnNivel % 2 == 1 ? MusicaManager.PELEA1 : MusicaManager.PELEA2);
            MusicaManager.Reproducir(pista);
        }

        public void Update(float dt)
        {
            // Si la ronda termino, correr el fundido y recien ahi cambiar de pantalla
            if (_terminando)
            {
                _fadeOut += FADE_VEL * dt;
                if (_fadeOut >= 1f)
                {
                    if (GameManager.Instance.Estado == GameState.Victory)
                        _sm.ChangeState(new VictoryState(_sm));
                    else
                        _sm.ChangeState(new SkillTreeState(_sm));
                }
                return;   // el combate queda congelado durante el fundido
            }

            // Pausar / reanudar con P
            _pauseCooldown -= dt;
            if (_pauseCooldown <= 0f && Engine.Engine.KeyPress(Engine.Engine.KEY_P))
            {
                _pausado = !_pausado;
                _pauseCooldown = 0.3f;
            }
            // Acelerar el tiempo con F (x1 -> x2 -> x3 -> x1)
            _velCooldown -= dt;
            if (_velCooldown <= 0f && Engine.Engine.KeyPress(Engine.Engine.KEY_F))
            {
                _velTiempo   = _velTiempo >= 3f ? 1f : _velTiempo + 1f;
                _velCooldown = 0.3f;
            }

            if (_pausado) return;   // el juego no avanza mientras esta en pausa

            // Tiempo de simulacion escalado por el multiplicador de velocidad
            float sim = dt * _velTiempo;

            // Spawn por hordas: rafagas de 1 a 4 enemigos con pausa entre ellas
            if (_enemigosSpawneados < _enemigosTotal)
            {
                _spawnTimer += sim;
                float intervalo = _hordaRestante > 0 ? DELAY_DENTRO_HORDA : DELAY_ENTRE_HORDAS;
                if (_spawnTimer >= intervalo)
                {
                    _spawnTimer = 0f;
                    if (_hordaRestante <= 0)
                    {
                        // Tamaño de horda proporcional a la ronda (hasta ~1/4 del
                        // total). En el nivel 1 los enemigos salen de a uno.
                        int maxHorda = 1;
                        if (GameManager.Instance.NivelActual >= 2)
                            maxHorda = System.Math.Max(2, _enemigosTotal / 4);

                        _hordaRestante = _rng.Next(1, maxHorda + 1);
                        int faltan = _enemigosTotal - _enemigosSpawneados;
                        if (_hordaRestante > faltan) _hordaRestante = faltan;
                    }
                    SpawnEnemy();
                    _hordaRestante--;
                }
            }

            // Actualizar enemigos
            foreach (var e in _enemies)
                e.Update(sim);

            // Actualizar torres (buscan, disparan y atacan enemigos)
            foreach (var t in _torres)
                t.Update(sim, _enemies, _balas);

            // Actualizar balas (el gestor recicla las muertas en su pool)
            _balas.Update(sim);

            // Limpiar muertos
            _enemies.RemoveAll(e => !e.IsAlive);

            // La derrota tiene prioridad: si el ultimo enemigo (o el boss) llega al
            // castillo y nos derrota, NO debe contarse como ronda ganada.
            if (GameManager.Instance.Estado == GameState.Defeat)
            {
                _sm.ChangeState(new DefeatState(_sm));
                return;
            }

            // Fin de oleada: arranca el fundido hacia el arbol de habilidades
            if (_enemigosSpawneados >= _enemigosTotal && _enemies.Count == 0)
            {
                GameManager.Instance.WaveComplete();
                _terminando = true;
            }
        }

        public void Render()
        {
            Engine.Engine.Clear();

            SDL.SDL_Rect destFondo = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 576 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref destFondo);

            // Muro decorativo en el gap de abajo (debajo del mapa). Se recorta solo
            // la banda de ladrillos (rows 95..144 del png) y se estira a todo el gap.
            if (_muro != null)
            {
                SDL.SDL_Rect srcMuro  = new SDL.SDL_Rect { x = 0, y = 95, w = 256, h = 49 };
                SDL.SDL_Rect destMuro = new SDL.SDL_Rect { x = 0, y = 576, w = 1024, h = 192 };
                SDL.SDL_RenderCopy(Engine.Engine.renderer, _muro.Pointer, ref srcMuro, ref destMuro);
            }

            foreach (var t in _torres)  t.Render();
            foreach (var e in _enemies) e.Render();
            _balas.Render();

            // Titulo grande de nivel/ronda, centrado arriba
            string titulo = $"NIVEL {GameManager.Instance.NivelActual}   -   RONDA {GameManager.Instance.RondaEnNivel}/{GameManager.RONDAS_POR_NIVEL}";
            int tituloW = Engine.Engine.TextWidth(titulo, _fontTitulo);
            Engine.Engine.DrawText(titulo, (1024 - tituloW) / 2, 10, 255, 255, 255, _fontTitulo);

            // HUD
            Engine.Engine.DrawText($"Oro: {GameManager.Instance.Monedas}",  10,  10, 255, 215,   0, _fontHud);
            Engine.Engine.DrawText($"Cristales: {GameManager.Instance.Cristales}", 10, 40, 120, 220, 255, _fontHud);
            Engine.Engine.DrawText($"Castillo: {GameManager.Instance.Hits}/{GameManager.Instance.MaxHits}",  10,  70, 255, 100, 100, _fontHud);
            Engine.Engine.DrawText($"Enemigos: {_enemigosEliminados}/{_enemigosTotal}", 10, 100, 200, 200, 200, _fontHud);
            _castle.Render();

            // Leyenda de controles (abajo)
            Engine.Engine.DrawText($"P: pausa    F: velocidad x{(int)_velTiempo}    +/-: volumen    ESC: salir",
                10, 730, 220, 220, 220, _fontHud);

            // Overlay de pausa
            if (_pausado)
            {
                Engine.Engine.DrawText("PAUSA", 430, 320, 255, 255, 255, _fontHud);
                Engine.Engine.DrawText("P para continuar", 400, 360, 200, 200, 200, _fontHud);
            }

            // Fundido de salida al ganar la ronda
            if (_terminando)
            {
                byte a = (byte)System.Math.Min(255f, _fadeOut * 255f);
                SDL.SDL_SetRenderDrawBlendMode(Engine.Engine.renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 0, 0, 0, a);
                SDL.SDL_Rect full = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref full);
            }

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
            EnemyType tipo = ElegirTipo();
            Image[] frames = FramesPara(tipo);
            // Salidas alternadas: cada enemigo sale por una ruta distinta en orden.
            var ruta = _map.Rutas[_enemigosSpawneados % _map.Rutas.Count];
            Enemy enemy = EnemyFactory.Create(tipo, ruta, frames);
            _enemies.Add(enemy);
            _enemigosSpawneados++;
        }

        private EnemyType ElegirTipo()
        {
            GameManager gm = GameManager.Instance;

            // Los bosses salen como ultimos enemigos de la ronda final del nivel.
            // La cantidad escala con el nivel (1, 1, 2, 3, 5...).
            if (gm.RondaEnNivel == GameManager.RONDAS_POR_NIVEL)
            {
                int nBosses = gm.CantidadBossesDelNivel();
                if (_enemigosSpawneados >= _enemigosTotal - nBosses)
                    return EnemyType.Boss;
            }

            // Mega tanque mezclado: mas probable a mayor nivel
            float probTank = (gm.NivelActual - 1) * 0.12f;
            if (_rng.NextDouble() < probTank)
                return EnemyType.MegaTank;

            // Resto: alternar soldado y drone
            return (_enemigosSpawneados % 3 == 2) ? EnemyType.Drone : EnemyType.Soldier;
        }

        private Image[] FramesPara(EnemyType tipo)
        {
            switch (tipo)
            {
                case EnemyType.Drone:    return _imgDrone;
                case EnemyType.MegaTank: return _imgMegaTank;
                case EnemyType.Boss:     return _imgBoss;
                default:                 return _imgSoldier;
            }
        }

        // Intenta cargar carpeta/0..3.png; si falta algun frame usa el fallback
        private Image[] CargarFrames(string carpeta, Image[] fallback)
        {
            Image[] f = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                Image img = Engine.Engine.LoadImageSafe($"{carpeta}/{i}.png");
                if (img == null) return fallback;
                f[i] = img;
            }
            return f;
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
