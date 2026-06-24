namespace ProyectoSDL2.Game.Managers
{
    // Estado de una torre dentro de la partida: si esta desbloqueada,
    // cuantas se entregan por ronda y sus niveles de mejora.
    public class DatosTorre
    {
        public bool Desbloqueada;
        public int  Cantidad;        // unidades de esta torre por ronda
        public int  NivelDano;
        public int  NivelRango;
        public int  NivelCadencia;
        public int  NivelRalentizacion;   // solo lo usa el mago
        public bool MejoraEspecial;       // mejora definitiva de la rama (late game)
    }
}
