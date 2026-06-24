using System.Collections.Generic;
using System.IO;
using ProyectoSDL2.Engine;

namespace ProyectoSDL2.Game.Managers
{
    // Maneja la musica de fondo por estado. Cachea cada pista y no reinicia la
    // que ya esta sonando (asi cambiar de estado no corta la musica si es la misma).
    public static class MusicaManager
    {
        private const string DIR = "Assets/Musica/";

        public const string PREPARACION = DIR + "MusicaPreparacion.mp3";
        public const string PELEA1      = DIR + "MusicaPelea1.mp3";
        public const string PELEA2      = DIR + "MusicaPelea2.mp3";
        public const string JEFE        = DIR + "MusicaJefe.mp3";
        public const string MEJORAS     = DIR + "MusicaMejoras.mp3";
        public const string VICTORIA    = DIR + "MusicaVictoria.mp3";
        public const string DERROTA     = DIR + "MusicaDerrota.mp3";

        private static readonly Dictionary<string, Sound> _cache = new Dictionary<string, Sound>();
        private static string _actual = null;

        // Volumen actual de la musica (0..128). Arranca a la mitad.
        private static int _volumen = 64;
        private const int PASO = 8;   // pasos chicos: ~16 niveles entre 0 y 128

        public static int Volumen => _volumen;

        public static void SubirVolumen() => FijarVolumen(_volumen + PASO);
        public static void BajarVolumen() => FijarVolumen(_volumen - PASO);

        public static void FijarVolumen(int v)
        {
            _volumen = System.Math.Clamp(v, 0, 128);
            Sound.Volumen(_volumen);
        }

        // Reproduce una pista en loop. Si ya es la que suena, no hace nada.
        public static void Reproducir(string ruta)
        {
            if (ruta == _actual) return;

            if (!_cache.TryGetValue(ruta, out Sound s))
            {
                if (!File.Exists(ruta)) return;   // si falta el archivo, queda en silencio
                s = new Sound(ruta);
                _cache[ruta] = s;
            }

            s.PlayLooping();
            _actual = ruta;
        }

        // Corta la musica (p. ej. al volver al menu).
        public static void Detener()
        {
            Sound.DetenerMusica();
            _actual = null;
        }
    }
}
