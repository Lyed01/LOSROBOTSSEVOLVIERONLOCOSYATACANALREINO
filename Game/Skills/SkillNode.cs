using System.Collections.Generic;

namespace ProyectoSDL2.Game.Skills
{
    // Con que moneda se paga el nodo
    public enum MonedaTipo { Oro, Cristal }

    // A que torre afecta el nodo (Castillo = mejora global de vida)
    public enum TorreObjetivo { Arquero, Hachero, Mago, Castillo }

    // Que hace el nodo al comprarlo
    public enum SkillAccion
    {
        Ninguno,
        DesbloquearTorre,
        MasCantidad,
        MasDano,
        MasRango,
        MasCadencia,
        MasRalentizacion,
        MejoraEspecial,
        VidaCastillo
    }

    public class SkillNode
    {
        public string        Nombre;
        public string        Descripcion;
        public int           Costo;
        public MonedaTipo    Moneda;
        public SkillAccion   Accion;
        public TorreObjetivo Torre;

        // Posicion en pantalla para dibujar la red
        public int X;
        public int Y;

        // Imagen del nodo sin comprar. Si no existe el archivo se dibuja un respaldo de color.
        public string ImagenRuta;

        // Imagen del nodo ya comprado (se muestra al estar Comprado). Puede ser null.
        public string ImagenComprada;

        public bool Comprado     = false;
        public bool Desbloqueado = false;   // se puede comprar

        // Indices de los nodos que deben estar COMPRADOS para habilitar este.
        // Un nodo sin requisitos (raiz) arranca desbloqueado.
        // El diseño es convergente: un nodo "Torre" lista como requisitos los
        // tres nodos del set de mejoras que lo preceden, asi los caminos se juntan.
        public List<int> Requisitos = new List<int>();

        public SkillNode(string nombre, string descripcion, int costo,
                         MonedaTipo moneda, SkillAccion accion, TorreObjetivo torre,
                         int x, int y, string imagenRuta = null)
        {
            Nombre      = nombre;
            Descripcion = descripcion;
            Costo       = costo;
            Moneda      = moneda;
            Accion      = accion;
            Torre       = torre;
            X           = x;
            Y           = y;
            ImagenRuta  = imagenRuta;
        }
    }
}
