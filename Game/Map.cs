using System.Collections.Generic;
using System.IO;

namespace ProyectoSDL2.Game
{
    // Mapa cargado desde una matriz de texto (Assets/Mapas/mapaN.txt).
    // La matriz define el camino, las salidas de enemigos y el castillo; las
    // rutas (waypoints) se calculan con BFS desde cada salida hasta el castillo.
    public class Map
    {
        public const int TILE = 64;
        public const int COLS = 16;   // 1024 / 64
        public const int ROWS = 9;    // 576  / 64 (zona jugable; abajo va el HUD)

        // Una ruta por cada salida de enemigos. Cada ruta es la lista de waypoints
        // desde la salida hasta el castillo.
        public List<List<Vector2>> Rutas { get; private set; } = new List<List<Vector2>>();

        // Ruta del fondo del mapa (lo cargan PlanningState y CombatState).
        public string Fondo { get; private set; }

        // Pixel del castillo (celda 'C'), para ubicar el sprite.
        public Vector2 Castillo { get; private set; }

        // Compatibilidad: la primera ruta.
        public List<Vector2> Waypoints => Rutas.Count > 0 ? Rutas[0] : new List<Vector2>();

        // Grid de ocupacion; true = camino/salida/castillo (no se puede construir).
        private bool[,] _ocupado = new bool[COLS, ROWS];

        public Map(string archivoMatriz, string fondo)
        {
            Fondo = fondo;
            char[,] celdas = LeerMatriz(archivoMatriz);
            Construir(celdas);
        }

        // ── Carga ──────────────────────────────────────────────────────────────

        // Lee la matriz de texto. Ignora lineas vacias y comentarios (';').
        // Devuelve una grilla [COLS, ROWS]; lo que falte se rellena con '.'.
        private static char[,] LeerMatriz(string ruta)
        {
            char[,] grid = new char[COLS, ROWS];
            for (int c = 0; c < COLS; c++)
                for (int r = 0; r < ROWS; r++)
                    grid[c, r] = '.';

            if (!File.Exists(ruta))
            {
                Engine.Engine.Debug($"Mapa no encontrado: {ruta}. Uso camino recto por defecto.");
                CaminoPorDefecto(grid);
                return grid;
            }

            var filas = new List<string>();
            foreach (string lineaCruda in File.ReadAllLines(ruta))
            {
                string linea = lineaCruda.TrimEnd('\r', '\n');
                if (linea.Length == 0) continue;
                if (linea.TrimStart().StartsWith(";")) continue;   // comentario
                filas.Add(linea);
                if (filas.Count >= ROWS) break;
            }

            for (int r = 0; r < filas.Count && r < ROWS; r++)
            {
                string fila = filas[r];
                for (int c = 0; c < COLS && c < fila.Length; c++)
                    grid[c, r] = fila[c];
            }

            return grid;
        }

        // Camino recto en la fila central, por si falta el archivo de matriz.
        private static void CaminoPorDefecto(char[,] grid)
        {
            int r = ROWS / 2;
            grid[0, r] = '1';
            for (int c = 1; c < COLS - 1; c++) grid[c, r] = '#';
            grid[COLS - 1, r] = 'C';
        }

        // ── Construccion del mapa a partir de la grilla ─────────────────────────

        private void Construir(char[,] celdas)
        {
            var salidas = new List<(int col, int row)>();   // '1' antes que '2'
            (int col, int row) castillo = (COLS - 1, ROWS / 2);
            bool hayCastillo = false;

            // Primero las salidas en orden ('1' y luego '2') para que la ruta A sea
            // siempre la de la salida 1 (importa para el spawn alternado).
            for (char marca = '1'; marca <= '2'; marca++)
                for (int r = 0; r < ROWS; r++)
                    for (int c = 0; c < COLS; c++)
                        if (celdas[c, r] == marca) salidas.Add((c, r));

            for (int r = 0; r < ROWS; r++)
                for (int c = 0; c < COLS; c++)
                {
                    char ch = celdas[c, r];
                    if (ch == 'C') { castillo = (c, r); hayCastillo = true; }
                    // Todo lo que no es terreno construible ocupa la celda.
                    if (ch != '.') _ocupado[c, r] = true;
                }

            Castillo = new Vector2(castillo.col * TILE, castillo.row * TILE);

            if (salidas.Count == 0) salidas.Add((0, ROWS / 2));
            if (!hayCastillo)
                Engine.Engine.Debug($"Matriz sin castillo ('C'); uso {castillo}.");

            // Una ruta por salida, trazando el corredor hasta el castillo.
            foreach (var s in salidas)
            {
                List<Vector2> ruta = TrazarRuta(celdas, s, castillo);
                if (ruta.Count > 0) Rutas.Add(ruta);
            }

            // Si ninguna ruta llego al castillo, dejar al menos una directa.
            if (Rutas.Count == 0)
                Rutas.Add(new List<Vector2> { Castillo });
        }

        // Camina el corredor desde la salida hasta el castillo. Reglas:
        //  - Inercia: por defecto sigue derecho en la direccion actual.
        //  - Curvas en L: si no puede seguir derecho y hay una sola salida, dobla.
        //  - Flechas (< > ^ v): en un cruce, mandan la direccion (resuelven la
        //    ambiguedad cuando ademas de doblar tambien podria seguir derecho).
        // Devuelve los waypoints (pixel de cada celda) desde la salida al castillo.
        private static List<Vector2> TrazarRuta(char[,] celdas, (int col, int row) inicio, (int col, int row) meta)
        {
            var ruta = new List<Vector2>();
            int col = inicio.col, row = inicio.row;
            int prevCol = -1, prevRow = -1;
            int dc = 0, dr = 0;                 // direccion actual (0,0 = sin definir)
            int maxPasos = COLS * ROWS * 3;     // tope de seguridad anti-bucle

            ruta.Add(new Vector2(col * TILE, row * TILE));

            for (int paso = 0; paso < maxPasos; paso++)
            {
                if (celdas[col, row] == 'C') break;   // llegamos al castillo

                int ndc = 0, ndr = 0;
                bool definido = false;

                var flecha = FlechaDir(celdas[col, row]);
                if (flecha != null)
                {
                    // Una flecha en esta celda manda la direccion de salida.
                    ndc = flecha.Value.dc; ndr = flecha.Value.dr; definido = true;
                }
                else if ((dc != 0 || dr != 0) && EnRango(col + dc, row + dr) && Transitable(celdas[col + dc, row + dr]))
                {
                    // Inercia: seguir derecho si se puede.
                    ndc = dc; ndr = dr; definido = true;
                }
                else
                {
                    // Buscar vecinos transitables sin volver por donde vine.
                    int[] ddc = { 1, -1, 0, 0 };
                    int[] ddr = { 0, 0, 1, -1 };
                    var cand = new List<(int dc, int dr)>();
                    for (int i = 0; i < 4; i++)
                    {
                        int nc = col + ddc[i], nr = row + ddr[i];
                        if (!EnRango(nc, nr)) continue;
                        if (nc == prevCol && nr == prevRow) continue;
                        if (!Transitable(celdas[nc, nr])) continue;
                        cand.Add((ddc[i], ddr[i]));
                    }

                    if (cand.Count == 1) { ndc = cand[0].dc; ndr = cand[0].dr; definido = true; }
                    else if (cand.Count == 0) break;   // sin salida
                    else
                    {
                        Engine.Engine.Debug($"Cruce ambiguo en ({col},{row}): falta una flecha. Corto la ruta aca.");
                        break;
                    }
                }

                if (!definido) break;
                int tc = col + ndc, tr = row + ndr;
                if (!EnRango(tc, tr) || !Transitable(celdas[tc, tr])) break;

                prevCol = col; prevRow = row;
                col = tc; row = tr;
                dc = ndc; dr = ndr;
                ruta.Add(new Vector2(col * TILE, row * TILE));
            }

            return ruta;
        }

        private static bool EnRango(int c, int r)
        {
            return c >= 0 && c < COLS && r >= 0 && r < ROWS;
        }

        // Delta (dc,dr) de una celda-flecha, o null si no es flecha.
        private static (int dc, int dr)? FlechaDir(char ch)
        {
            switch (ch)
            {
                case '>': return (1, 0);
                case '<': return (-1, 0);
                case '^': return (0, -1);
                case 'v':
                case 'V': return (0, 1);
                default:  return null;
            }
        }

        private static bool Transitable(char ch)
        {
            return ch == '#' || ch == '1' || ch == '2' || ch == 'C'
                || ch == '<' || ch == '>' || ch == '^' || ch == 'v' || ch == 'V';
        }

        // ── Seleccion de mapa por nivel ─────────────────────────────────────────
        // Nivel 1 -> mapa 1 (1 salida). Niveles 2-3 -> mapa 2 (2 salidas).
        // Niveles 4-5 (y mas) -> mapa 3 (2 salidas).
        public static Map ParaNivel(int nivel)
        {
            const string carpeta = "Assets/Mapas/";
            if (nivel <= 1) return new Map(carpeta + "mapa1.txt", carpeta + "fondo.png");
            if (nivel <= 3) return new Map(carpeta + "mapa2.txt", carpeta + "Fondo2.png");
            return new Map(carpeta + "mapa3.txt", carpeta + "Fondo3.png");
        }

        // ── Grilla de torres ────────────────────────────────────────────────────

        public bool CanPlaceTower(int pixelX, int pixelY)
        {
            int col = pixelX / TILE;
            int row = pixelY / TILE;
            if (col < 0 || col >= COLS || row < 0 || row >= ROWS) return false;
            return !_ocupado[col, row];
        }

        public void OccupyTile(int pixelX, int pixelY)
        {
            int col = pixelX / TILE;
            int row = pixelY / TILE;
            if (col >= 0 && col < COLS && row >= 0 && row < ROWS)
                _ocupado[col, row] = true;
        }

        // Snap a la grilla
        public (int, int) SnapToGrid(int pixelX, int pixelY)
        {
            return (pixelX / TILE * TILE, pixelY / TILE * TILE);
        }
    }
}
