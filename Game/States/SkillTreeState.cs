using ProyectoSDL2.Engine;
using ProyectoSDL2.Game.Entities;
using ProyectoSDL2.Game.Interfaces;
using ProyectoSDL2.Game.Managers;
using ProyectoSDL2.Game.Skills;
using System.Collections.Generic;
using System.IO;
using SDL2;

namespace ProyectoSDL2.Game.States
{
    // Pantalla del arbol de habilidades entre rondas.
    // Cuatro ramas (arquero / hachero / mago / castillo) con sets de mejoras
    // que convergen en nodos "Torre". Columna de datos a la derecha.
    public class SkillTreeState : IGameState
    {
        private StateManager _sm;
        private Font         _fontTitulo;
        private Font         _fontTexto;

        // Imagenes de los nodos cargadas por ruta (las que existen en disco)
        private Dictionary<string, Image> _imagenes = new Dictionary<string, Image>();
        private Image _fondo;   // null si no hay archivo de fondo

        private const string RUTA_FONDO = "Assets/PantallaFondoMenuMejoras.png";

        // Indice del nodo seleccionado (directo sobre la lista del arbol)
        private int _sel = 0;

        // Anti-repeticion de teclas
        private float _timer = 0.3f;
        private const float DELAY = 0.15f;

        // Mensaje de feedback al intentar comprar
        private string _mensaje      = "";
        private float  _mensajeTimer = 0f;

        // Fundido de entrada (la pantalla aparece desde negro)
        private float _fadeIn = 1f;
        private const float FADE_VEL = 1.8f;

        // Confirmacion al salir sin haber comprado nada en esta visita
        private bool _comproAlgo        = false;
        private bool _confirmandoSalida = false;

        // Lado del cuadrado de cada nodo (compacto para que entren las 4 ramas)
        private const int NODO = 44;

        // Las torres y el mapa se resetean cada ronda, por eso no se reciben aca:
        // al continuar se crea un PlanningState nuevo y limpio.
        public SkillTreeState(StateManager sm)
        {
            _sm = sm;
        }

        private List<SkillNode> Nodos => GameManager.Instance.Arbol.Nodos;

        public void Enter()
        {
            _fontTitulo = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 24);
            _fontTexto  = Engine.Engine.LoadFont("Assets/Fonts/pixel.ttf", 16);
            GameManager.Instance.SetState(GameState.SkillTree);

            // Cargar imagenes que existan (si falta el archivo se usa el respaldo)
            List<SkillNode> nodos = Nodos;
            for (int i = 0; i < nodos.Count; i++)
            {
                CargarIcono(nodos[i].ImagenRuta);
                CargarIcono(nodos[i].ImagenComprada);
            }
            if (File.Exists(RUTA_FONDO))
                _fondo = Engine.Engine.LoadImage(RUTA_FONDO);

            MusicaManager.Reproducir(MusicaManager.MEJORAS);
        }

        public void Update(float dt)
        {
            if (_fadeIn > 0f) _fadeIn = System.Math.Max(0f, _fadeIn - FADE_VEL * dt);
            if (_mensajeTimer > 0f) _mensajeTimer -= dt;

            _timer -= dt;
            if (_timer > 0) return;

            // Navegacion espacial entre nodos
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_LEFT))  { MoverSeleccion(-1,  0); _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_RIGHT)) { MoverSeleccion( 1,  0); _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_UP))    { MoverSeleccion( 0, -1); _timer = DELAY; }
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_DOWN))  { MoverSeleccion( 0,  1); _timer = DELAY; }

            // Comprar el nodo seleccionado
            if (Engine.Engine.KeyPress(SDL.SDL_Keycode.SDLK_RETURN))
            {
                IntentarComprar();
                _timer = DELAY;
            }

            GameManager gm = GameManager.Instance;

            // Opciones extra solo al final del nivel
            if (gm.EsFinDeNivel())
            {
                if (Engine.Engine.KeyPress(Engine.Engine.KEY_R))
                {
                    gm.RepetirNivel();
                    _sm.ChangeState(new PlanningState(_sm));
                    return;
                }
            }

            // Continuar: avanza de ronda o de nivel.
            // Si no se compro ninguna mejora, pide confirmar antes de salir.
            if (Engine.Engine.KeyPress(Engine.Engine.KEY_ESP))
            {
                if (!_comproAlgo && !_confirmandoSalida)
                {
                    _confirmandoSalida = true;
                    Avisar("Sin mejoras compradas. ESPACIO de nuevo para continuar");
                    _timer = DELAY;
                    return;
                }

                gm.Continuar();
                if (gm.Estado == GameState.Victory)
                    _sm.ChangeState(new VictoryState(_sm));
                else
                    _sm.ChangeState(new PlanningState(_sm));
            }
        }

        // Intenta comprar el nodo seleccionado y deja un mensaje de feedback.
        private void IntentarComprar()
        {
            SkillNode n = Nodos[_sel];
            if (n.Comprado)            { Avisar("Ya comprado");  return; }
            if (!n.Desbloqueado)       { Avisar("Bloqueado");    return; }

            bool ok = GameManager.Instance.Arbol.Comprar(_sel);
            if (ok)
            {
                _comproAlgo        = true;
                _confirmandoSalida = false;   // ya no hace falta confirmar
                Avisar("Comprado!");
            }
            else
            {
                Avisar(n.Moneda == MonedaTipo.Oro ? "Sin oro" : "Sin cristales");
            }
        }

        private void Avisar(string texto)
        {
            _mensaje      = texto;
            _mensajeTimer = 1.5f;
        }

        // Mueve la seleccion al nodo mas cercano en la direccion indicada.
        private void MoverSeleccion(int dirX, int dirY)
        {
            List<SkillNode> nodos = Nodos;
            SkillNode actual = nodos[_sel];

            int   mejor      = -1;
            float mejorScore = float.MaxValue;

            for (int i = 0; i < nodos.Count; i++)
            {
                if (i == _sel) continue;
                float dx = nodos[i].X - actual.X;
                float dy = nodos[i].Y - actual.Y;

                // Cuanto avanza en la direccion deseada (descarta lo que va al reves)
                float along = dx * dirX + dy * dirY;
                if (along <= 0) continue;

                // Penalizar la desviacion perpendicular para preferir el alineado
                float perp  = (dirX != 0) ? System.Math.Abs(dy) : System.Math.Abs(dx);
                float score = along + perp * 2f;
                if (score < mejorScore) { mejorScore = score; mejor = i; }
            }

            if (mejor >= 0) _sel = mejor;
        }

        public void Render()
        {
            Engine.Engine.Clear();

            // Fondo: imagen si existe, si no un color oscuro
            if (_fondo != null)
            {
                SDL.SDL_Rect dest = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
                SDL.SDL_RenderCopy(Engine.Engine.renderer, _fondo.Pointer, IntPtr.Zero, ref dest);
            }
            else
            {
                Rect(0, 0, 1024, 768, 25, 25, 40);
            }

            List<SkillNode> nodos = Nodos;

            // Lineas de cada requisito hacia el nodo (muestran la convergencia)
            for (int i = 0; i < nodos.Count; i++)
            {
                SkillNode n = nodos[i];
                foreach (int r in n.Requisitos)
                {
                    SkillNode req = nodos[r];
                    // Verde si el requisito ya esta comprado, gris si falta
                    if (req.Comprado)
                        SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 90, 180, 110, 255);
                    else
                        SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 80, 80, 95, 255);
                    SDL.SDL_RenderDrawLine(Engine.Engine.renderer, req.X, req.Y, n.X, n.Y);
                }
            }

            // Nodos (todos visibles, coloreados por estado)
            for (int i = 0; i < nodos.Count; i++)
            {
                SkillNode n = nodos[i];
                int x = n.X - NODO / 2;
                int y = n.Y - NODO / 2;

                // Color de estado: verde comprado / amarillo disponible / gris bloqueado
                byte r, g, b;
                if (n.Comprado)          { r = 60;  g = 200; b = 80;  }
                else if (n.Desbloqueado) { r = 200; g = 170; b = 60;  }
                else                     { r = 80;  g = 80;  b = 95;  }

                // Imagen del nodo (comprado o base) o respaldo de color
                string rutaImg = (n.Comprado && n.ImagenComprada != null) ? n.ImagenComprada : n.ImagenRuta;
                Image img = ObtenerImagen(rutaImg);
                if (img != null)
                {
                    SDL.SDL_Rect dest = new SDL.SDL_Rect { x = x, y = y, w = NODO, h = NODO };
                    SDL.SDL_RenderCopy(Engine.Engine.renderer, img.Pointer, IntPtr.Zero, ref dest);
                }
                else
                {
                    Rect(x, y, NODO, NODO, r, g, b);
                }

                // Borde de color segun estado
                RectBorde(x, y, NODO, NODO, r, g, b);

                // Borde blanco si es el seleccionado
                if (i == _sel)
                    RectBorde(x - 3, y - 3, NODO + 6, NODO + 6, 255, 255, 255);
            }

            RenderPanel();

            // Fundido de entrada
            if (_fadeIn > 0f)
            {
                byte a = (byte)System.Math.Min(255f, _fadeIn * 255f);
                SDL.SDL_SetRenderDrawBlendMode(Engine.Engine.renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, 0, 0, 0, a);
                SDL.SDL_Rect full = new SDL.SDL_Rect { x = 0, y = 0, w = 1024, h = 768 };
                SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref full);
            }

            Engine.Engine.Show();
        }

        public void Exit() { }

        // ── Panel derecho con los datos ────────────────────────────────────────
        // Todos los textos van centrados en el eje horizontal de la zona derecha.
        private const int PANEL_CENTRO = 882;   // medio entre x=740 y x=1024

        private void RenderPanel()
        {
            GameManager gm = GameManager.Instance;

            DrawCentrado("MEJORAS", 30, 255, 255, 255, _fontTitulo);
            DrawCentrado($"Nivel {gm.NivelActual} - Ronda {gm.RondaEnNivel}/{GameManager.RONDAS_POR_NIVEL}",
                70, 220, 220, 220, _fontTexto);
            DrawCentrado($"Oro: {gm.Monedas}",         100, 255, 215, 0,   _fontTexto);
            DrawCentrado($"Cristales: {gm.Cristales}", 125, 120, 220, 255, _fontTexto);

            // Datos del nodo seleccionado
            SkillNode n = Nodos[_sel];
            string moneda = n.Moneda == MonedaTipo.Oro ? "Oro" : "Cristal";

            DrawCentrado(n.Nombre, 180, 255, 255, 255, _fontTexto);
            DrawCentrado(n.Descripcion, 205, 200, 200, 200, _fontTexto);
            if (n.Comprado)
                DrawCentrado("Comprado", 240, 60, 220, 90, _fontTexto);
            else if (!n.Desbloqueado)
                DrawCentrado("Bloqueado", 240, 160, 160, 170, _fontTexto);
            else
                DrawCentrado($"Costo: {n.Costo} {moneda}", 240, 255, 215, 0, _fontTexto);

            // Mensaje de feedback de la ultima compra
            if (_mensajeTimer > 0f)
                DrawCentrado(_mensaje, 270, 255, 230, 120, _fontTexto);

            // Ayuda de controles
            DrawCentrado("Flechas: mover", 320, 160, 160, 160, _fontTexto);
            DrawCentrado("ENTER: comprar", 345, 160, 160, 160, _fontTexto);

            // Boton al final del nivel: repetir
            if (gm.EsFinDeNivel())
                DrawCentrado("REPETIR NIVEL (R)", 570, 255, 255, 255, _fontTexto);

            // Boton continuar (solo texto, sin recuadro)
            DrawCentrado("CONTINUAR (ESPACIO)", 695, 255, 255, 255, _fontTexto);
        }

        // Dibuja un texto centrado horizontalmente en PANEL_CENTRO.
        private void DrawCentrado(string texto, int y, byte r, byte g, byte b, Font font)
        {
            int w = Engine.Engine.TextWidth(texto, font);
            Engine.Engine.DrawText(texto, PANEL_CENTRO - w / 2, y, r, g, b, font);
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        // Carga un icono a la cache si existe el archivo (lo ignora si falta).
        private void CargarIcono(string ruta)
        {
            if (ruta != null && !_imagenes.ContainsKey(ruta) && File.Exists(ruta))
                _imagenes[ruta] = Engine.Engine.LoadImage(ruta);
        }

        private Image ObtenerImagen(string ruta)
        {
            if (ruta != null && _imagenes.ContainsKey(ruta))
                return _imagenes[ruta];
            return null;
        }

        private void Rect(int x, int y, int w, int h, byte r, byte g, byte b)
        {
            SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, r, g, b, 255);
            SDL.SDL_Rect rect = new SDL.SDL_Rect { x = x, y = y, w = w, h = h };
            SDL.SDL_RenderFillRect(Engine.Engine.renderer, ref rect);
        }

        private void RectBorde(int x, int y, int w, int h, byte r, byte g, byte b)
        {
            SDL.SDL_SetRenderDrawColor(Engine.Engine.renderer, r, g, b, 255);
            SDL.SDL_Rect rect = new SDL.SDL_Rect { x = x, y = y, w = w, h = h };
            SDL.SDL_RenderDrawRect(Engine.Engine.renderer, ref rect);
        }
    }
}
