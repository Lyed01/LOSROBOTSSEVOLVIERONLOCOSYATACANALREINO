using System;
using System.Collections.Generic;

namespace ProyectoSDL2.Game.Pooling
{
    // POOL GENERICO de objetos reutilizables.
    //
    // Sirve para CUALQUIER tipo T que sea una clase, implemente IPoolable y
    // tenga constructor sin parametros (o se le pase una fabrica). En lugar de
    // crear y destruir objetos constantemente (lo que genera basura para el GC),
    // el pool mantiene una reserva de instancias y las recicla:
    //   - Obtener()  -> devuelve una instancia lista para usar (reutilizada o nueva)
    //   - Devolver() -> guarda la instancia para volver a usarla mas tarde
    //
    // Cumple el principio Open/Closed: se puede poolear un tipo nuevo sin tocar
    // esta clase, solo implementando IPoolable.
    public class Pool<T> where T : class, IPoolable, new()
    {
        private readonly Stack<T> _libres = new Stack<T>();
        private readonly Func<T>  _fabrica;

        // Cantidad de objetos actualmente en uso (entregados y todavia no devueltos).
        public int EnUso { get; private set; }

        // Cantidad de objetos libres listos para reutilizar.
        public int Disponibles => _libres.Count;

        // precarga: cuantas instancias crear de antemano para evitar asignaciones
        //           durante el juego.
        // fabrica:  forma opcional de construir T (por defecto usa new T()).
        public Pool(int precarga = 0, Func<T> fabrica = null)
        {
            _fabrica = fabrica ?? (() => new T());

            for (int i = 0; i < precarga; i++)
                _libres.Push(_fabrica());
        }

        // Saca una instancia del pool. Si no quedan libres, crea una nueva.
        // Siempre la "reactiva" llamando AlObtener() antes de entregarla.
        public T Obtener()
        {
            T obj = _libres.Count > 0 ? _libres.Pop() : _fabrica();
            obj.AlObtener();
            EnUso++;
            return obj;
        }

        // Devuelve una instancia al pool para reutilizarla. La "apaga" llamando
        // AlDevolver() para que libere referencias.
        public void Devolver(T obj)
        {
            if (obj == null) return;

            obj.AlDevolver();
            _libres.Push(obj);
            if (EnUso > 0) EnUso--;
        }
    }
}
