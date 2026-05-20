using System.Collections.Generic;

namespace ProyectoSDL2.Game
{
    // Mapa con waypoints hardcodeados y grid de casillas para torres
    public class Map
    {
        public List<Vector2> Waypoints { get; private set; }

        // Grid de 64x64 px; true = ocupada por camino o torre
        private bool[,] _ocupado = new bool[16, 12];  // 1024/64 x 768/64

        public const int TILE = 64;

        public Map()
        {
            // Camino predefinido: izquierda → zigzag → castillo (derecha)
            Waypoints = new List<Vector2>
            {
                new Vector2(  0, 300),
                new Vector2(200, 300),
                new Vector2(200, 150),
                new Vector2(500, 150),
                new Vector2(500, 500),
                new Vector2(750, 500),
                new Vector2(750, 300),
                new Vector2(1024, 300),
            };

            // Marcar casillas del camino como ocupadas
            foreach (var wp in Waypoints)
            {
                int col = (int)(wp.X / TILE);
                int row = (int)(wp.Y / TILE);
                if (col >= 0 && col < 16 && row >= 0 && row < 12)
                    _ocupado[col, row] = true;
            }
        }

        public bool CanPlaceTower(int pixelX, int pixelY)
        {
            int col = pixelX / TILE;
            int row = pixelY / TILE;
            if (col < 0 || col >= 16 || row < 0 || row >= 12) return false;
            return !_ocupado[col, row];
        }

        public void OccupyTile(int pixelX, int pixelY)
        {
            int col = pixelX / TILE;
            int row = pixelY / TILE;
            if (col >= 0 && col < 16 && row >= 0 && row < 12)
                _ocupado[col, row] = true;
        }

        // Snap a la grilla
        public (int, int) SnapToGrid(int pixelX, int pixelY)
        {
            return (pixelX / TILE * TILE, pixelY / TILE * TILE);
        }
    }
}
