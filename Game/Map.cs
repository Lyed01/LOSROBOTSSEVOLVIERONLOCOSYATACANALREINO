using System.Collections.Generic;

namespace ProyectoSDL2.Game
{
    // Mapa con waypoints hardcodeados y grid de casillas para torres
    public class Map
    {
        public List<Vector2> Waypoints { get; private set; }

        // Grid de 64x64 px; true = ocupada por camino o torre
        private bool[,] _ocupado = new bool[16, 9];  // 1024/64 x 768/64

        public const int TILE = 64;

        public Map()
        {
            // Camino predefinido: izquierda → zigzag → castillo (derecha)
            Waypoints = new List<Vector2>
            {
            new Vector2(  0*64, 4*64), // E1
            new Vector2(  1*64, 4*64), // E2
            new Vector2(  2*64, 4*64), // E3
            new Vector2(  3*64, 4*64), // E4
            new Vector2(  3*64, 3*64), // D4
            new Vector2(  3*64, 2*64), // C4
            new Vector2(  3*64, 1*64), // B4
            new Vector2(  4*64, 1*64), // B5
            new Vector2(  5*64, 1*64), // B6
            new Vector2(  6*64, 1*64), // B7
            new Vector2(  6*64, 2*64), // C7
            new Vector2(  6*64, 3*64), // D7
            new Vector2(  6*64, 4*64), // E7
            new Vector2(  6*64, 5*64), // F7
            new Vector2(  6*64, 6*64), // G7
            new Vector2(  5*64, 6*64), // G6
            new Vector2(  4*64, 6*64), // G5
            new Vector2(  4*64, 7*64), // H5
            new Vector2(  4*64, 8*64), // I5
            new Vector2(  5*64, 8*64), // I6
            new Vector2(  6*64, 8*64), // I7
            new Vector2(  7*64, 8*64), // I8
            new Vector2(  8*64, 8*64), // I9
            new Vector2(  8*64, 7*64), // H9
            new Vector2(  8*64, 6*64), // G9
            new Vector2(  8*64, 5*64), // F9
            new Vector2(  8*64, 4*64), // E9
            new Vector2(  8*64, 3*64), // D9
            new Vector2(  9*64, 3*64), // D10
            new Vector2( 10*64, 3*64), // D11
            new Vector2( 11*64, 3*64), // D12
            new Vector2( 12*64, 3*64), // D13
            new Vector2( 13*64, 3*64), // D14
            new Vector2( 14*64, 3*64), // D15
            new Vector2( 14*64, 4*64), // E15
            new Vector2( 14*64, 5*64), // F15
            new Vector2( 14*64, 6*64), // G15
            new Vector2( 14*64, 7*64), // H15
            new Vector2( 13*64, 7*64), // H14
            new Vector2( 12*64, 7*64), // H13
            new Vector2( 11*64, 7*64), // H12
            new Vector2( 11*64, 6*64), // G12
            new Vector2( 11*64, 5*64), // F12
            new Vector2( 11*64, 4*64), // E12
            new Vector2( 11*64, 3*64), // D12
            new Vector2( 11*64, 2*64), // C12
            new Vector2( 11*64, 1*64), // B12
            new Vector2( 12*64, 1*64), // B13
            new Vector2( 13*64, 1*64), // B14
            new Vector2( 14*64, 1*64), // B15
            new Vector2( 15*64, 1*64), // B16 — CASTILLO
            };

            // Marcar todas las celdas del camino como ocupadas
            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                int col1 = (int)(Waypoints[i].X / TILE);
                int row1 = (int)(Waypoints[i].Y / TILE);
                int col2 = (int)(Waypoints[i + 1].X / TILE);
                int row2 = (int)(Waypoints[i + 1].Y / TILE);

                int minCol = System.Math.Min(col1, col2);
                int maxCol = System.Math.Max(col1, col2);
                int minRow = System.Math.Min(row1, row2);
                int maxRow = System.Math.Max(row1, row2);

                for (int col = minCol; col <= maxCol; col++)
                    for (int row = minRow; row <= maxRow; row++)
                        if (col >= 0 && col < 16 && row >= 0 && row < 9)
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
