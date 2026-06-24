namespace ProyectoSDL2.Game.Pooling
{
    // Contrato que debe cumplir cualquier objeto que quiera ser administrado
    // por un Pool<T> generico. Define los dos momentos del ciclo de vida del
    // objeto dentro del pool: cuando se reactiva y cuando se recicla.
    public interface IPoolable
    {
        // Se invoca al SACAR el objeto del pool (reinicia su estado a "vivo").
        void AlObtener();

        // Se invoca al DEVOLVER el objeto al pool (libera referencias y lo apaga).
        void AlDevolver();
    }
}
