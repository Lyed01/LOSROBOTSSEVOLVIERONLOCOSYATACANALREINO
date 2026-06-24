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
        private Image _muro;   // decoracion del gap de abajo (puede ser null)
        private StateManager _sm;
        private Map _map;
        private List<Tower> _torres;
        private Font _fontHud;
        private Font _fontTitulo;
        private Image _imgArcherTower;
        private Image _imgAxeTower;
        private Image _imgMageTower;
        private Image _imgBullet;
        private Image[] _framesFuego;     // animacion del haz del mago (puede ser null)
        private Image[] _framesHacha;     // animacion de ataque del hachero (puede ser null)

        private const int TILE = Map.TILE;

        // Torres que quedan por colocar esta ronda, por tipo
        private int _restArquero = 0;
        private int _restHachero = 0;
        private int _restMago    = 0;

        // Cursor en la grilla
        private int _cursorCol = 0;
        private int _cursorRow = 0;

        // Torre seleccionada: 0 = Arquero, 1 = Hacha, 2 = Mago
        private int _seleccion = 0;

        // Direccion del haz al colocar el mago
        private Direccion _direccionMago = Direccion.Derecha;

        // Anti-repetición
        private float _timer = 0.3f;
        private const float DELAY = 0.15f;

        // Aviso temporal en pantalla (p. ej. "colocá una torre")
        private string _aviso      = "";
        private float  _avisoTimer = 0f;

        public PlanningState(StateManager sm, Map map = null, List<Tower> torres = null)
        {
            _sm = sm;
            // Si no viene un mapa, elegir el que corresponde al nivel actual.
            _map = map ?? Map.ParaNivel(GameManager.Instance.NivelActual);
            _torres = torres ?? new List<Tower>();
        }

        public void Enter()
        {
            _fondo = Engine.Engine.LoadImage(_map.Fondo);
            _muro  = Engine.Engine.LoadImageSafe("Assets/Mapas/MuroAbajoDelMapa.png");
            _fontHud    = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 20);
            _fontTitulo = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 38);
            _imgArcherTower = Engine.Engine.LoadImage("Assets/sprites/archer_tower.png");
            _imgAxeTower    = Engine.Engine.LoadImage("Assets/sprites/axe_tower.png");
            _imgBullet      = Engine.Engine.LoadImage("Assets/bullet.png");

            // Sprites opcionales (si faltan se usa un respaldo). El mago reusa el
            // sprite del arquero como placeholder mientras no exista su asset.
            _imgMageTower = Engine.Engine.LoadImageSafe("Assets/sprites/mage_tower.png") ?? _imgArcherTower;
            _framesFuego  = CargarFramesFuego();
            _framesHacha  = CargarFramesHacha();

            // Cada ronda se entregan torres segun la cantidad de cada tipo
            GameManager gm = GameManager.Instance;
            _restArquero = gm.Arquero.Cantidad;
            _restHachero = gm.Hachero.Cantidad;
            _restMago    = gm.Mago.Cantidad;

            MusicaManager.Reproducir(MusicaManager.PREPARACION);
        }

        public void Update(float dt)
        {
            if (_avisoTimer > 0f) _avisoTimer -= dt;

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

            GameManager gm = GameManager.Instance;

            // Q rota la torre seleccionada entre las desbloqueadas
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_Q))
            {
                _seleccion = SiguienteTorreDesbloqueada(_seleccion);
                _timer = DELAY;
            }

            // Rotar la direccion del haz del mago con R
            if (_seleccion == 2 && Engine.Engine.KeyPress(Engine.Engine.KEY_R))
            {
                _direccionMago = SiguienteDireccion(_direccionMago);
                _timer = DELAY;
            }

            // Colocar torre con ENTER (se gasta una torre del tipo seleccionado)
            if (Engine.Engine.KeyPress(SDL2.SDL.SDL_Keycode.SDLK_RETURN))
            {
                int px = _cursorCol * TILE;
                int py = _cursorRow * TILE;

                if (RestanteSeleccion() > 0 && _map.CanPlaceTower(px, py))
                {
                    _torres.Add(CrearTorreSeleccionada(px, py));
                    _map.OccupyTile(px, py);
                    DescontarSeleccion();
                }
                _timer = DELAY;
            }

            // Comenzar oleada con ESPACIO (no se puede sin al menos una torre)
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_ESP))
            {
                if (_torres.Count > 0)
                    _sm.ChangeState(new CombatState(_sm, _map, _torres));
                else
                {
                    _aviso = "Colocá al menos una torre para comenzar";
                    _avisoTimer = 2f;
                }
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

            // Rango/area de cada torre ya colocada (debajo de los sprites)
            foreach (var t in _torres) DibujarRangoTorre(t);

            foreach (var t in _torres) t.Render();

            // Marcar con un cuadrito dorado las torres que tienen mejoras
            foreach (var t in _torres)
            {
                if (!t.Mejorada) continue;
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 255, 215, 0, 255);
                SDL.SDL_Rect marca = new SDL.SDL_Rect { x = t.X + TILE / 2 - 6, y = t.Y - 8, w = 12, h = 12 };
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref marca);
            }


            // Torre fantasma en el cursor
            int cx = _cursorCol * TILE;
            int cy = _cursorRow * TILE;
            Image ghost = _imgArcherTower;
            if (_seleccion == 1) ghost = _imgAxeTower;
            else if (_seleccion == 2) ghost = _imgMageTower;

            // Previsualizar el rango/area de la torre a colocar (contempla mejoras)
            if (_seleccion == 0)
                DibujarCirculo(cx + TILE / 2, cy + TILE / 2, RangoArqueroEfectivo(), 0, 220, 255);
            else if (_seleccion == 1)
                DibujarRectArea(cx, cy, GameManager.Instance.Hachero.MejoraEspecial ? 2 : 1, 255, 160, 60);
            else if (_seleccion == 2)
            {
                (int hx, int hy, int hw, int hh) = AreaHazGhost(cx, cy);
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 255, 140, 0, 255);
                SDL.SDL_Rect haz = new SDL.SDL_Rect { x = hx, y = hy, w = hw, h = hh };
                SDL.SDL_RenderDrawRect(Engine.Engine.renderer, ref haz);
            }

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

            // ── HUD inferior ──────────────────────────────────────────────────
            GameManager gm = GameManager.Instance;

            // Cantidades restantes por tipo (solo las desbloqueadas)
            string disponibles = $"Arquero: {_restArquero}";
            if (gm.Hachero.Desbloqueada) disponibles += $"   Hachero: {_restHachero}";
            if (gm.Mago.Desbloqueada)    disponibles += $"   Mago: {_restMago}";
            Engine.Engine.DrawText(disponibles, 10, 586, 255, 215, 0, _fontHud);

            // Titulo grande de nivel/ronda, centrado arriba
            string titulo = $"NIVEL {gm.NivelActual}   -   RONDA {gm.RondaEnNivel}/{GameManager.RONDAS_POR_NIVEL}";
            int tituloW = Engine.Engine.TextWidth(titulo, _fontTitulo);
            Engine.Engine.DrawText(titulo, (1024 - tituloW) / 2, 8, 255, 255, 255, _fontTitulo);

            // Aviso temporal (centrado)
            if (_avisoTimer > 0f)
            {
                int aw = Engine.Engine.TextWidth(_aviso, _fontHud);
                Engine.Engine.DrawText(_aviso, (1024 - aw) / 2, 540, 255, 120, 120, _fontHud);
            }

            // Pop-up de la torre seleccionada, en la parte baja de la pantalla
            DibujarPopupTorre();

            // Leyenda de controles abajo de todo, en color negro
            Engine.Engine.DrawText(
                "Q: cambiar torre   R: rotar mago   ENTER: colocar   ESPACIO: comenzar",
                10, 742, 0, 0, 0, _fontHud);

            Engine.Engine.Show();
        }

        public void Exit() { }

        // ── Helpers ────────────────────────────────────────────────────────────

        // Pop-up en la parte baja con el nombre y la descripcion de la torre
        // seleccionada (un panel con fondo y borde, centrado).
        private void DibujarPopupTorre()
        {
            string nombre = NombreTorre(_seleccion);
            if (_seleccion == 2) nombre += $"   (haz: {NombreDireccion(_direccionMago)})";
            string desc = DescripcionTorre(_seleccion);

            int pw = 700, ph = 80;
            int px = (1024 - pw) / 2;   // centrado horizontalmente
            int py = 612;

            IntPtr r = Engine.Engine.renderer;
            SDL.SDL_SetRenderDrawBlendMode(r, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            // Fondo oscuro semitransparente
            SDL.SDL_Rect panel = new SDL.SDL_Rect { x = px, y = py, w = pw, h = ph };
            SDL.SDL_SetRenderDrawColor(r, 20, 20, 30, 220);
            SDL.SDL_RenderFillRect(r, ref panel);

            // Borde dorado
            SDL.SDL_SetRenderDrawColor(r, 255, 215, 0, 255);
            SDL.SDL_RenderDrawRect(r, ref panel);

            // Nombre (dorado) y descripcion (gris claro), centrados en el panel
            int nw = Engine.Engine.TextWidth(nombre, _fontHud);
            Engine.Engine.DrawText(nombre, px + (pw - nw) / 2, py + 12, 255, 215, 0, _fontHud);
            int dw = Engine.Engine.TextWidth(desc, _fontHud);
            Engine.Engine.DrawText(desc, px + (pw - dw) / 2, py + 46, 230, 230, 230, _fontHud);
        }

        private string NombreTorre(int sel)
        {
            if (sel == 0) return "ARQUERO";
            if (sel == 1) return "HACHERO";
            return "MAGO";
        }

        private string DescripcionTorre(int sel)
        {
            if (sel == 0) return "Dispara flechas que persiguen al enemigo.";
            if (sel == 1) return "Golpe en area 3x3 (incluye diagonales).";
            return "Haz de fuego recto de 3 casillas.";
        }

        private int RestanteSeleccion()
        {
            if (_seleccion == 0) return _restArquero;
            if (_seleccion == 1) return _restHachero;
            return _restMago;
        }

        private void DescontarSeleccion()
        {
            if (_seleccion == 0) _restArquero--;
            else if (_seleccion == 1) _restHachero--;
            else _restMago--;
        }

        private Tower CrearTorreSeleccionada(int px, int py)
        {
            if (_seleccion == 0)
                return TowerFactory.Create(TowerType.Archer, px, py, _imgArcherTower, _imgBullet);
            if (_seleccion == 1)
                return TowerFactory.Create(TowerType.Axe, px, py, _imgAxeTower, null,
                    Direccion.Derecha, null, _framesHacha);
            // Mago
            return TowerFactory.Create(TowerType.Mage, px, py, _imgMageTower, null,
                _direccionMago, _framesFuego);
        }

        private Direccion SiguienteDireccion(Direccion d)
        {
            switch (d)
            {
                case Direccion.Arriba:    return Direccion.Derecha;
                case Direccion.Derecha:   return Direccion.Abajo;
                case Direccion.Abajo:     return Direccion.Izquierda;
                default:                  return Direccion.Arriba;
            }
        }

        private string NombreDireccion(Direccion d)
        {
            switch (d)
            {
                case Direccion.Arriba:    return "arriba";
                case Direccion.Abajo:     return "abajo";
                case Direccion.Izquierda: return "izquierda";
                default:                  return "derecha";
            }
        }

        private (int, int, int, int) AreaHazGhost(int px, int py)
        {
            return AreaHaz(px, py, _direccionMago);
        }

        // Rectangulo del haz del mago (3 tiles) segun posicion y direccion.
        private (int, int, int, int) AreaHaz(int px, int py, Direccion dir)
        {
            int largo = 3 * TILE;
            switch (dir)
            {
                case Direccion.Derecha:   return (px + TILE, py, largo, TILE);
                case Direccion.Izquierda: return (px - largo, py, largo, TILE);
                case Direccion.Arriba:    return (px, py - largo, TILE, largo);
                default:                  return (px, py + TILE, TILE, largo); // Abajo
            }
        }

        // ── Seleccion de torre y previsualizacion de rangos ────────────────────

        // Devuelve el indice de la siguiente torre desbloqueada (cicla 0->1->2->0).
        private int SiguienteTorreDesbloqueada(int actual)
        {
            GameManager gm = GameManager.Instance;
            for (int i = 1; i <= 2; i++)
            {
                int cand = (actual + i) % 3;
                bool ok = cand == 0
                       || (cand == 1 && gm.Hachero.Desbloqueada)
                       || (cand == 2 && gm.Mago.Desbloqueada);
                if (ok) return cand;
            }
            return actual;
        }

        // Rango del arquero con las mejoras del arbol (200 base + bono por nivel).
        private int RangoArqueroEfectivo()
        {
            return (int)(200f + GameManager.Instance.Arquero.NivelRango * GameManager.BONO_RANGO);
        }

        // Dibuja el rango/area de una torre ya colocada segun su tipo.
        private void DibujarRangoTorre(Tower t)
        {
            int cx = t.X + TILE / 2;
            int cy = t.Y + TILE / 2;
            if (t is ArcherTower)
                DibujarCirculo(cx, cy, (int)t.Rango, 0, 220, 255);
            else if (t is AxeTower)
                DibujarRectArea(t.X, t.Y, GameManager.Instance.Hachero.MejoraEspecial ? 2 : 1, 255, 160, 60);
            else if (t is MageTower mago)
            {
                (int hx, int hy, int hw, int hh) = AreaHaz(t.X, t.Y, mago.Dir);
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 255, 140, 0, 255);
                SDL.SDL_Rect haz = new SDL.SDL_Rect { x = hx, y = hy, w = hw, h = hh };
                SDL.SDL_RenderDrawRect(Engine.Engine.renderer, ref haz);
            }
        }

        // Cuadrado centrado en la casilla con un radio en tiles (area del hachero).
        // radio 1 = 3x3, radio 2 = 5x5 (Onda Expansiva).
        private void DibujarRectArea(int x, int y, int radio, byte r, byte g, byte b)
        {
            int lado = (2 * radio + 1) * TILE;
            SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, r, g, b, 255);
            SDL.SDL_Rect rect = new SDL.SDL_Rect { x = x - radio * TILE, y = y - radio * TILE, w = lado, h = lado };
            SDL.SDL_RenderDrawRect(Engine.Engine.renderer, ref rect);
        }

        // Aproxima un circulo con segmentos de linea (rango del arquero).
        private void DibujarCirculo(int cx, int cy, int radio, byte r, byte g, byte b)
        {
            SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, r, g, b, 255);
            const int segs = 48;
            double paso = 2 * System.Math.PI / segs;
            int px = cx + radio, py = cy;
            for (int i = 1; i <= segs; i++)
            {
                int nx = cx + (int)(radio * System.Math.Cos(i * paso));
                int ny = cy + (int)(radio * System.Math.Sin(i * paso));
                SDL.SDL_RenderDrawLine(Engine.Engine.renderer, px, py, nx, ny);
                px = nx; py = ny;
            }
        }

        // Carga los frames del haz del mago si existen; si no, null.
        private Image[] CargarFramesFuego()
        {
            string[] nombres =
            {
                "Assets/sprites/Fires/fire.png",
                "Assets/sprites/Fires/Fire2.png",
                "Assets/sprites/Fires/Fire3.png",
            };
            Image[] f = new Image[nombres.Length];
            for (int i = 0; i < nombres.Length; i++)
            {
                Image img = Engine.Engine.LoadImageSafe(nombres[i]);
                if (img == null) return null;
                f[i] = img;
            }
            return f;
        }

        // Carga los 4 frames del golpe del hachero si existen; si no, null.
        private Image[] CargarFramesHacha()
        {
            string[] nombres =
            {
                "Assets/sprites/Axe Sprites/AtaqueHacha.png",
                "Assets/sprites/Axe Sprites/AtaqueHacha1.png",
                "Assets/sprites/Axe Sprites/AtaqueHacha2.png",
                "Assets/sprites/Axe Sprites/AtaqueHacha3.png",
            };
            Image[] f = new Image[nombres.Length];
            for (int i = 0; i < nombres.Length; i++)
            {
                Image img = Engine.Engine.LoadImageSafe(nombres[i]);
                if (img == null) return null;
                f[i] = img;
            }
            return f;
        }
    }
}