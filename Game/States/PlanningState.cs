using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using ProyectoSDL2.Game.Factories;
using ProyectoSDL2.Game.Managers;
using System.Collections.Generic;
using ProyectoSDL2.Game.Interfaces;
using SDL2;
namespace ProyectoSDL2.Game.States
{
    public class PlanningState : IGameState
    {
        private Image _fondo;
        private StateManager _sm;
        private Map _map;
        private List<Tower> _torres;
        private Font _fontHud;
        private Image _imgArcherTower;
        private Image _imgAxeTower;
        private Image _imgBullet;

        private const int COSTO_ARCHER = 50;
        private const int COSTO_AXE = 75;
        private const int TILE = Map.TILE;

        // Cursor en la grilla
        private int _cursorCol = 0;
        private int _cursorRow = 0;

        // Torre seleccionada: 0 = Arquero, 1 = Hacha
        private int _seleccion = 0;

        // Anti-repetición
        private float _timer = 0.3f;
        private const float DELAY = 0.15f;

        public PlanningState(StateManager sm, Map map = null, List<Tower> torres = null)
        {
            _sm = sm;
            _map = map ?? new Map();
            _torres = torres ?? new List<Tower>();
        }

        public void Enter()
        {
            _fondo = Engine.Engine.LoadImage("Assets/fondo.png");
            _fontHud = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 20);
            _imgArcherTower = Engine.Engine.LoadImage("Assets/sprites/archer_tower.png");
            _imgAxeTower    = Engine.Engine.LoadImage("Assets/sprites/axe_tower.png");
            _imgBullet      = Engine.Engine.LoadImage("Assets/bullet.png");
        }

        public void Update(float dt)
        {
            _timer -= dt;
            if (_timer > 0) return;

            // Mover cursor
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_UP)) { _cursorRow--; _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_DOWN)) { _cursorRow++; _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_LEFT)) { _cursorCol--; _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_RIGHT)) { _cursorCol++; _timer = DELAY; }

            // Limitar cursor al mapa
            _cursorCol = System.Math.Clamp(_cursorCol, 0, 1024 / TILE - 1);
            _cursorRow = System.Math.Clamp(_cursorRow, 0, 600 / TILE - 1);

            // Cambiar torre seleccionada con Q/E
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_Q)) { _seleccion = 0; _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_E)) { _seleccion = 1; _timer = DELAY; }

            // Colocar torre con ENTER
            if (Engine.Engine.KeyPress(SDL2.SDL.SDL_Keycode.SDLK_RETURN))
            {
                int px = _cursorCol * TILE;
                int py = _cursorRow * TILE;
                int costo = _seleccion == 0 ? COSTO_ARCHER : COSTO_AXE;

                if (GameManager.Instance.Monedas >= costo && _map.CanPlaceTower(px, py))
                {
                    Image sheet = _seleccion == 0 ? _imgArcherTower : _imgAxeTower;
                    TowerType tipo = _seleccion == 0 ? TowerType.Archer : TowerType.Axe;
                    Image bullet = _seleccion == 0 ? _imgBullet : null;
                    _torres.Add(TowerFactory.Create(tipo, px, py, sheet, bullet));
                    _map.OccupyTile(px, py);
                    GameManager.Instance.SpendCoins(costo);
                }
                _timer = DELAY;
            }

            // Comenzar oleada con ESPACIO
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_ESP))
                _sm.ChangeState(new CombatState(_sm, _map, _torres));
        }

        public void Render()
        {
            Engine.Engine.Clear();
            SDL.SDL_Rect destFondo = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 576 };
            SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref destFondo);
            foreach (var t in _torres) t.Render();


            // Torre fantasma en el cursor
            int cx = _cursorCol * TILE;
            int cy = _cursorRow * TILE;
            Image ghost = _seleccion == 0 ? _imgArcherTower : _imgAxeTower;

            SDL.SDL_Rect destGhost = new SDL.SDL_Rect { x = cx, y = cy, w = 64, h = 64 };

            SDL.SDL_RenderCopy(Engine.Engine.renderer, ghost.Pointer, IntPtr.Zero, ref destGhost);


            // Borde del cursor: verde=válido, rojo=inválido
            bool valido = _map.CanPlaceTower(cx, cy);
            SDL2.SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer,
                valido ? (byte)0 : (byte)255,
                valido ? (byte)255 : (byte)0,
                0, 255);
            SDL2.SDL.SDL_Rect borde = new SDL2.SDL.SDL_Rect { x = cx, y = cy, w = TILE, h = TILE };
            SDL2.SDL.SDL_RenderDrawRect(Engine.Engine.renderer, ref borde);

            // HUD
            string torre = _seleccion == 0 ? $"Arquero ({COSTO_ARCHER})" : $"Hachero ({COSTO_AXE})";
          Engine.Engine.DrawText($"Monedas: {GameManager.Instance.Monedas}", 10, 586, 255, 215, 0, _fontHud);
          Engine.Engine.DrawText($"Torre: {torre}", 10, 616, 255, 255, 255, _fontHud);
          Engine.Engine.DrawText("Flechas: mover   Q/E: torre   ENTER: colocar   ESPACIO: comenzar", 10, 646, 160, 160, 160, _fontHud);

            Engine.Engine.Show();
        }

        public void Exit() { }
    }
}