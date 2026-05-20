using ProyectoSDL2.Game.Interfaces;
using System;

namespace ProyectoSDL2.Game.Managers
{
    public class GameManager
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static GameManager _instance;
        private static readonly object _lock = new object();
        private GameManager() { }

        public static GameManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GameManager();
                    return _instance;
                }
            }
        }

        // ── Datos globales ─────────────────────────────────────────────────────
        public int       Monedas        { get; private set; } = 100;
        public int       Hits           { get; private set; } = 3;   // vida del castillo
        public GameState Estado         { get; private set; } = GameState.Menu;
        public int       OleadaActual   { get; private set; } = 1;
        public int       TotalOleadas   { get; private set; } = 3;
        public int       PuntajeAcumulado { get; private set; } = 0;

        // ── Eventos ────────────────────────────────────────────────────────────
        public event Action<int> OnEnemyDied;      // arg: monedas que otorga el enemigo
        public event Action      OnCastleHit;
        public event Action      OnWaveComplete;
        public event Action<IGameState> OnStateChanged;

        // ── Operaciones ────────────────────────────────────────────────────────
        public void SetState(GameState state)
        {
            Estado = state;
        }

        public void AddCoins(int amount)
        {
            Monedas += amount;
        }

        public void SpendCoins(int amount)
        {
            Monedas -= amount;
        }

        public void TakeCastleHit()
        {
            Hits--;
            OnCastleHit?.Invoke();
            if (Hits <= 0)
                SetState(GameState.Defeat);
        }

        public void EnemyDied(int coins)
        {
            Monedas       += coins;
            PuntajeAcumulado += coins;
            OnEnemyDied?.Invoke(coins);
        }

        public void WaveComplete()
        {
            OnWaveComplete?.Invoke();
            if (OleadaActual >= TotalOleadas)
                SetState(GameState.Victory);
            else
            {
                OleadaActual++;
                SetState(GameState.SkillTree);
            }
        }

        public void NextWave()
        {
            SetState(GameState.Planning);
        }

        public void ResetGame()
        {
            Monedas         = 100;
            Hits            = 3;
            Estado          = GameState.Menu;
            OleadaActual    = 1;
            PuntajeAcumulado = 0;
        }
    }
}
