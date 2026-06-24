using System;
using ProyectoSDL2.Game.Skills;

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
        // Oro inicial bajo: arrancar apretado para que cada compra cuente.
        public const int  ORO_INICIAL    = 50;
        public int       Monedas        { get; private set; } = ORO_INICIAL;  // oro
        public int       Cristales      { get; private set; } = 0;    // moneda especial
        public int       Hits           { get; private set; } = 3;   // vida actual del castillo
        public int       MaxHits        { get; private set; } = 3;   // vida maxima (sube con el arbol)
        public GameState Estado         { get; private set; } = GameState.Menu;
        public int       PuntajeAcumulado { get; private set; } = 0;

        // ── Niveles y rondas ───────────────────────────────────────────────────
        // Cada nivel tiene 3 rondas. La partida avanza de nivel hasta TotalNiveles.
        public const int RONDAS_POR_NIVEL = 3;
        public int       NivelActual  { get; private set; } = 1;
        public int       RondaEnNivel { get; private set; } = 1;   // 1..3
        public int       TotalNiveles { get; private set; } = 5;

        // Arbol de habilidades (persiste entre rondas)
        public SkillTree Arbol { get; private set; } = new SkillTree();

        // ── Datos por tipo de torre ────────────────────────────────────────────
        // Cada torre tiene su propio desbloqueo, cantidad por ronda y niveles.
        // El arquero arranca desbloqueado con 1; las demas se desbloquean en el arbol.
        public DatosTorre Arquero { get; private set; } = new DatosTorre { Desbloqueada = true,  Cantidad = 1 };
        public DatosTorre Hachero { get; private set; } = new DatosTorre { Desbloqueada = false, Cantidad = 0 };
        public DatosTorre Mago    { get; private set; } = new DatosTorre { Desbloqueada = false, Cantidad = 0 };

        // Cuanto suma cada nivel de mejora
        public const float BONO_DANO     = 10f;
        public const float BONO_RANGO    = 30f;
        public const float BONO_CADENCIA = 0.3f;

        // Ralentizacion del mago: cada nivel frena un poco mas a los enemigos.
        // El factor de velocidad resultante es 1 - NivelRalentizacion * BONO_RALENTIZACION
        // (con piso en SLOW_MINIMO). La duracion del slow por tick esta en SLOW_DURACION.
        public const float BONO_RALENTIZACION = 0.15f;
        public const float SLOW_MINIMO        = 0.35f;
        public const float SLOW_DURACION      = 0.6f;

        // Devuelve los datos de la torre indicada
        public DatosTorre DatosDe(TorreObjetivo t)
        {
            switch (t)
            {
                case TorreObjetivo.Hachero: return Hachero;
                case TorreObjetivo.Mago:    return Mago;
                default:                    return Arquero;
            }
        }

        // ── Eventos ────────────────────────────────────────────────────────────
        public event Action<int> OnEnemyDied;
        public event Action      OnCastleHit;
        public event Action      OnWaveComplete;

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

        public void AddCristales(int amount)
        {
            Cristales += amount;
        }

        // ── Mejoras por torre que aplica el arbol de habilidades ───────────────
        public void DesbloquearTorre(TorreObjetivo t)
        {
            DatosTorre d = DatosDe(t);
            d.Desbloqueada = true;
            if (d.Cantidad < 1) d.Cantidad = 1;   // el desbloqueo da la primera unidad
        }

        public void SumarCantidad(TorreObjetivo t)      { DatosDe(t).Cantidad++; }
        public void SubirDano(TorreObjetivo t)          { DatosDe(t).NivelDano++; }
        public void SubirRango(TorreObjetivo t)         { DatosDe(t).NivelRango++; }
        public void SubirCadencia(TorreObjetivo t)      { DatosDe(t).NivelCadencia++; }
        public void SubirRalentizacion(TorreObjetivo t) { DatosDe(t).NivelRalentizacion++; }
        public void ActivarEspecial(TorreObjetivo t)    { DatosDe(t).MejoraEspecial = true; }

        // Factor de velocidad (0..1) que aplica el mago segun su nivel de ralentizacion.
        // Devuelve 1 (sin efecto) si todavia no se compro la mejora.
        public float FactorSlowMago()
        {
            int nivel = Mago.NivelRalentizacion;
            if (nivel <= 0) return 1f;
            return System.Math.Max(SLOW_MINIMO, 1f - nivel * BONO_RALENTIZACION);
        }

        // Mejora de vida del castillo
        public void SubirVidaCastillo() { MaxHits++; Hits++; }

        // Restaura la vida del castillo al maximo (al empezar cada ronda)
        public void RestaurarCastillo() { Hits = MaxHits; }

        // Derrota inmediata (por ejemplo si el boss llega al castillo)
        public void PerderPartida() { SetState(GameState.Defeat); }

        public void SpendCristales(int amount)
        {
            Cristales -= amount;
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

        // Al terminar una ronda siempre se pasa por la pantalla de mejoras
        public void WaveComplete()
        {
            OnWaveComplete?.Invoke();
            SetState(GameState.SkillTree);
        }

        // La ronda recien terminada era la ultima del nivel
        public bool EsFinDeNivel()
        {
            return RondaEnNivel >= RONDAS_POR_NIVEL;
        }

        // Boton "Continuar": avanza de ronda o de nivel
        public void Continuar()
        {
            if (EsFinDeNivel())
            {
                if (NivelActual >= TotalNiveles)
                {
                    SetState(GameState.Victory);
                    return;
                }
                NivelActual++;
                RondaEnNivel = 1;
                RestaurarCastillo();   // el castillo solo se recupera al subir de nivel
            }
            else
            {
                RondaEnNivel++;
                // El daño al castillo PERSISTE entre rondas del mismo nivel:
                // filtrar enemigos ahora pesa en las rondas siguientes.
            }
            SetState(GameState.Planning);
        }

        // Boton "Repetir nivel" / reintentar: vuelve a la ronda 1 del mismo nivel
        public void RepetirNivel()
        {
            RondaEnNivel = 1;
            RestaurarCastillo();
            SetState(GameState.Planning);
        }

        // Boton "Nivel anterior": retrocede un nivel para juntar recursos
        public void NivelAnterior()
        {
            if (NivelActual > 1) NivelActual--;
            RondaEnNivel = 1;
            RestaurarCastillo();
            SetState(GameState.Planning);
        }

        // ── Escalado de dificultad (curva fija, tuneable) ──────────────────────
        // La cantidad arranca baja (acorde a tener pocas torres) y sube fuerte
        // por nivel, mas suave por ronda. N1R1 = 4 (la mitad del 8 anterior).
        public const int ENEMIGOS_BASE      = 4;
        public const int ENEMIGOS_POR_NIVEL = 7;   // crece mas en niveles superiores
        public const int ENEMIGOS_POR_RONDA = 2;

        // La vida crece de gran manera: exponencial por nivel y lineal por ronda.
        public const float VIDA_FACTOR_NIVEL = 1.45f;   // x1.45 de vida por cada nivel
        public const float VIDA_POR_RONDA    = 0.15f;   // +15% extra por ronda del nivel

        // La velocidad tambien sube con los niveles (mas presion sobre las torres).
        public const float VEL_POR_NIVEL = 0.12f;       // +12% de velocidad por nivel

        // Cantidad de enemigos de la ronda actual
        public int EnemigosDeLaRonda()
        {
            return ENEMIGOS_BASE
                 + (NivelActual  - 1) * ENEMIGOS_POR_NIVEL
                 + (RondaEnNivel - 1) * ENEMIGOS_POR_RONDA;
        }

        // Multiplicador de vida de los enemigos segun el progreso (crece fuerte)
        public float MultiplicadorVida()
        {
            float porNivel = (float)Math.Pow(VIDA_FACTOR_NIVEL, NivelActual - 1);
            float porRonda = 1f + (RondaEnNivel - 1) * VIDA_POR_RONDA;
            return porNivel * porRonda;
        }

        // Multiplicador de velocidad de los enemigos segun el nivel
        public float MultiplicadorVelocidad()
        {
            return 1f + (NivelActual - 1) * VEL_POR_NIVEL;
        }

        // Cantidad de bosses que salen al final de la ronda final del nivel.
        // Niveles 1-2: 1. Nivel 3: 2. Nivel 4: 3. Nivel 5 (o mas): 5.
        public int CantidadBossesDelNivel()
        {
            switch (NivelActual)
            {
                case 1:
                case 2:  return 1;
                case 3:  return 2;
                case 4:  return 3;
                default: return 5;   // nivel 5 en adelante
            }
        }

        // ── Atajos de debug (teclas F1..F3 / F12) ──────────────────────────────
        // Saltar directo a un nivel: deja la ronda en 1 y restaura el castillo.
        // Lo usa el bucle principal para mandar al arbol antes de ese nivel.
        public void IrANivel(int nivel)
        {
            NivelActual  = System.Math.Clamp(nivel, 1, TotalNiveles);
            RondaEnNivel = 1;
            RestaurarCastillo();
        }

        // Inyectar muchos recursos para testear (oro y cristales).
        public void DarRecursosDebug()
        {
            Monedas   += 100000;
            Cristales += 100000;
        }

        public void ResetGame()
        {
            Monedas         = ORO_INICIAL;
            Cristales       = 0;
            Hits            = 3;
            MaxHits         = 3;
            Estado          = GameState.Menu;
            NivelActual     = 1;
            RondaEnNivel    = 1;
            PuntajeAcumulado = 0;
            // El arquero arranca disponible; hachero y mago se desbloquean en el arbol.
            Arquero         = new DatosTorre { Desbloqueada = true,  Cantidad = 1 };
            Hachero         = new DatosTorre { Desbloqueada = false, Cantidad = 0 };
            Mago            = new DatosTorre { Desbloqueada = false, Cantidad = 0 };
            Arbol           = new SkillTree();
        }
    }
}
