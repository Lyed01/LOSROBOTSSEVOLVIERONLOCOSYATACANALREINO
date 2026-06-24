using System.Collections.Generic;
using System.Linq;
using ProyectoSDL2.Game.Managers;

namespace ProyectoSDL2.Game.Skills
{
    // Red de habilidades convergente: cada rama tiene sets de mejoras
    // (daño / rango / cadencia segun la torre) y al completar un set se
    // habilita un nodo "Torre" que da +1 unidad por ronda. Los caminos se juntan.
    public class SkillTree
    {
        public List<SkillNode> Nodos = new List<SkillNode>();

        // ── Iconos de cada nodo (dos estados: base / comprado) ─────────────────
        // El icono se deduce del tipo de nodo (accion + torre) en Nodo(); no hace
        // falta pasarlo a mano. Si un archivo no existe, se dibuja el respaldo de
        // color. Algunos todavia faltan (Daño Hachero base, Torre Maga base,
        // Ultimate Mago base/comprada) y se veran con el color hasta agregarlos.
        private const string DIR_BASE = "Assets/skills/SkillsBase/";
        private const string DIR_COMP = "Assets/skills/SkillsCompradas/";

        // Icono del nodo SIN comprar segun su accion y torre.
        private static string RutaBase(SkillAccion a, TorreObjetivo t)
        {
            switch (t)
            {
                case TorreObjetivo.Arquero:
                    switch (a)
                    {
                        case SkillAccion.MasDano:        return DIR_BASE + "MejoraTorreArqueraDaño.png";
                        case SkillAccion.MasRango:       return DIR_BASE + "MejoraArqueraRango.png";
                        case SkillAccion.MasCadencia:    return DIR_BASE + "MejoraArqueraCadencia.png";
                        case SkillAccion.MejoraEspecial: return DIR_BASE + "MejoraUltimateArquera.png";
                        default:                         return DIR_BASE + "MejoraTorreArquera.png";
                    }
                case TorreObjetivo.Hachero:
                    switch (a)
                    {
                        case SkillAccion.MasDano:        return DIR_BASE + "MejoraDañoHachero.png";
                        case SkillAccion.MasCadencia:    return DIR_BASE + "MejoraHacheraCadencia.png";
                        case SkillAccion.MejoraEspecial: return DIR_BASE + "MejoraUltimateHachera.png";
                        default:                         return DIR_BASE + "MejoraTorreHachera.png";
                    }
                case TorreObjetivo.Mago:
                    switch (a)
                    {
                        case SkillAccion.MasDano:          return DIR_BASE + "MejoraMagaDaño.png";
                        case SkillAccion.MasCadencia:      return DIR_BASE + "MejoraMagaCadencia.png";
                        case SkillAccion.MasRalentizacion: return DIR_BASE + "MejoraRalentizacionMago.png";
                        case SkillAccion.MejoraEspecial:   return DIR_BASE + "MejoraUltimateMago.png"; // falta
                        default:                           return DIR_BASE + "MejoraTorreMaga.png";    // falta
                    }
                default: // Castillo
                    return DIR_BASE + "MejoraCastillo+.png";
            }
        }

        // Icono del nodo YA comprado.
        private static string RutaComprada(SkillAccion a, TorreObjetivo t)
        {
            switch (t)
            {
                case TorreObjetivo.Arquero:
                    switch (a)
                    {
                        case SkillAccion.MasDano:        return DIR_COMP + "MejoraArqueraDañoComprada.png";
                        case SkillAccion.MasRango:       return DIR_COMP + "MejoraArqueraRangoComprada.png";
                        case SkillAccion.MasCadencia:    return DIR_COMP + "MejoraArqueraCadenciaComprada.png";
                        case SkillAccion.MejoraEspecial: return DIR_COMP + "MejoraUltimateArqueraComprada.png";
                        default:                         return DIR_COMP + "MejorTorreArqueraComprada.png";
                    }
                case TorreObjetivo.Hachero:
                    switch (a)
                    {
                        case SkillAccion.MasDano:        return DIR_COMP + "MejoraDañoHachaComprada.png";
                        case SkillAccion.MasCadencia:    return DIR_COMP + "MejoraCadenciaHachaComprada.png";
                        case SkillAccion.MejoraEspecial: return DIR_COMP + "MejoraUltimateHacheraComprada.png";
                        default:                         return DIR_COMP + "MejoraTorreHacheraComprada.png";
                    }
                case TorreObjetivo.Mago:
                    switch (a)
                    {
                        case SkillAccion.MasDano:          return DIR_COMP + "MejoraDañoMagoComprada.png";
                        case SkillAccion.MasCadencia:      return DIR_COMP + "MejoraCadenciaMagoComprada.png";
                        case SkillAccion.MasRalentizacion: return DIR_COMP + "MejoraRalentizacionMagoComprada.png";
                        case SkillAccion.MejoraEspecial:   return DIR_COMP + "MejoraUltimateMagoComprada.png"; // falta
                        default:                           return DIR_COMP + "MejoraTorreMagaComprada.png";
                    }
                default: // Castillo
                    return DIR_COMP + "MejoraCastilloComprado.png";
            }
        }

        // ── Layout (filas/columnas, en pixeles) ────────────────────────────────
        private const int Y0 = 55;   // y de la primera fila
        private const int DY = 84;   // separacion entre filas

        public SkillTree()
        {
            Construir();
            ActualizarDesbloqueos();
        }

        private int Fila(int f) => Y0 + f * DY;

        // Crea un nodo, lo agrega y devuelve su indice. El icono (base y comprado)
        // se deduce de la accion y la torre.
        private int Nodo(string nombre, string desc, int costo, MonedaTipo moneda,
                         SkillAccion accion, TorreObjetivo torre, int x, int y)
        {
            var n = new SkillNode(nombre, desc, costo, moneda, accion, torre, x, y, RutaBase(accion, torre));
            n.ImagenComprada = RutaComprada(accion, torre);
            Nodos.Add(n);
            return Nodos.Count - 1;
        }

        private void Construir()
        {
            // ── Rama Arquero (D/R/C, ya disponible) ────────────────────────────
            const int xA0 = 60, xA1 = 140, xA2 = 220;

            int aBase = Nodo("Arquero", "Torre base", 0,
                MonedaTipo.Oro, SkillAccion.Ninguno, TorreObjetivo.Arquero, xA1, Fila(0));
            Nodos[aBase].Comprado = true;

            // Segunda torre temprana: accesible en la primera visita al arbol,
            // antes de invertir en mejoras. Cuesta oro (no cristal) para no
            // depender del recurso escaso en el arranque del juego.
            int aT0 = Nodo("+Arquero", "Un arquero mas por ronda", 30,
                MonedaTipo.Oro, SkillAccion.MasCantidad, TorreObjetivo.Arquero, xA1, Fila(1));
            Req(aT0, aBase);

            int a1d = Nodo("Dano Arq.", "Mas dano del arquero", 25,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Arquero, xA0, Fila(2));
            int a1r = Nodo("Rango Arq.", "Mas rango del arquero", 25,
                MonedaTipo.Oro, SkillAccion.MasRango, TorreObjetivo.Arquero, xA1, Fila(2));
            int a1c = Nodo("Cadencia Arq.", "El arquero ataca mas seguido", 25,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Arquero, xA2, Fila(2));
            Req(a1d, aT0); Req(a1r, aT0); Req(a1c, aT0);

            int aT1 = Nodo("+Arquero II", "Otro arquero mas por ronda", 6,
                MonedaTipo.Cristal, SkillAccion.MasCantidad, TorreObjetivo.Arquero, xA1, Fila(3));
            Req(aT1, a1d); Req(aT1, a1r); Req(aT1, a1c);

            int a2d = Nodo("Dano Arq. II", "Mas dano del arquero", 45,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Arquero, xA0, Fila(4));
            int a2r = Nodo("Rango Arq. II", "Mas rango del arquero", 45,
                MonedaTipo.Oro, SkillAccion.MasRango, TorreObjetivo.Arquero, xA1, Fila(4));
            int a2c = Nodo("Cadencia Arq. II", "El arquero ataca mas seguido", 45,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Arquero, xA2, Fila(4));
            Req(a2d, aT1); Req(a2r, aT1); Req(a2c, aT1);

            int a3d = Nodo("Dano Arq. III", "Mas dano del arquero", 70,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Arquero, xA0, Fila(5));
            int a3r = Nodo("Rango Arq. III", "Mas rango del arquero", 70,
                MonedaTipo.Oro, SkillAccion.MasRango, TorreObjetivo.Arquero, xA1, Fila(5));
            int a3c = Nodo("Cadencia Arq. III", "El arquero ataca mas seguido", 70,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Arquero, xA2, Fila(5));
            Req(a3d, a2d); Req(a3r, a2r); Req(a3c, a2c);

            int aT2 = Nodo("+Arquero III", "Otro arquero mas por ronda", 9,
                MonedaTipo.Cristal, SkillAccion.MasCantidad, TorreObjetivo.Arquero, xA1, Fila(6));
            Req(aT2, a3d); Req(aT2, a3r); Req(aT2, a3c);

            // Mejora definitiva del arquero (cara, late game)
            int aEsp = Nodo("Disparo Doble", "El arquero ataca a 2 enemigos a la vez", 130,
                MonedaTipo.Oro, SkillAccion.MejoraEspecial, TorreObjetivo.Arquero, xA1, Fila(7));
            Req(aEsp, aT2);

            // ── Rama Hachero (D/C) ─────────────────────────────────────────────
            const int xH0 = 300, xH1 = 380, xHb = 340;

            int hBase = Nodo("Desbloq. Hachero", "Habilita la torre hachera", 4,
                MonedaTipo.Cristal, SkillAccion.DesbloquearTorre, TorreObjetivo.Hachero, xHb, Fila(0));

            int h1d = Nodo("Dano Hach.", "Mas dano del hachero", 35,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Hachero, xH0, Fila(1));
            int h1c = Nodo("Cadencia Hach.", "El hachero ataca mas seguido", 35,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Hachero, xH1, Fila(1));
            Req(h1d, hBase); Req(h1c, hBase);

            int hT1 = Nodo("+Hachero", "Un hachero mas por ronda", 5,
                MonedaTipo.Cristal, SkillAccion.MasCantidad, TorreObjetivo.Hachero, xHb, Fila(2));
            Req(hT1, h1d); Req(hT1, h1c);

            int h2d = Nodo("Dano Hach. II", "Mas dano del hachero", 60,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Hachero, xH0, Fila(3));
            int h2c = Nodo("Cadencia Hach. II", "El hachero ataca mas seguido", 60,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Hachero, xH1, Fila(3));
            Req(h2d, hT1); Req(h2c, hT1);

            // Mejora definitiva del hachero: area de golpe 5x5
            int hEsp = Nodo("Onda Expansiva", "El hachero golpea un area de 5x5", 140,
                MonedaTipo.Oro, SkillAccion.MejoraEspecial, TorreObjetivo.Hachero, xHb, Fila(4));
            Req(hEsp, h2d); Req(hEsp, h2c);

            // ── Rama Mago (D/C + Ralentizacion) ────────────────────────────────
            const int xM0 = 440, xM1 = 520, xM2 = 600;

            int mBase = Nodo("Desbloq. Mago", "Habilita la torre de mago", 5,
                MonedaTipo.Cristal, SkillAccion.DesbloquearTorre, TorreObjetivo.Mago, xM1, Fila(0));

            int m1d = Nodo("Dano Mago", "Mas dano del mago", 35,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Mago, xM0, Fila(1));
            int m1c = Nodo("Cadencia Mago", "El mago ataca mas seguido", 35,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Mago, xM2, Fila(1));
            Req(m1d, mBase); Req(m1c, mBase);

            int mT1 = Nodo("+Mago", "Un mago mas por ronda", 5,
                MonedaTipo.Cristal, SkillAccion.MasCantidad, TorreObjetivo.Mago, xM1, Fila(2));
            Req(mT1, m1d); Req(mT1, m1c);

            int m2d = Nodo("Dano Mago II", "Mas dano del mago", 50,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Mago, xM0, Fila(3));
            int m2c = Nodo("Cadencia Mago II", "El mago ataca mas seguido", 50,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Mago, xM1, Fila(3));
            int m2r = Nodo("Ralentizacion", "El haz frena a los enemigos", 4,
                MonedaTipo.Cristal, SkillAccion.MasRalentizacion, TorreObjetivo.Mago, xM2, Fila(3));
            Req(m2d, mT1); Req(m2c, mT1); Req(m2r, mT1);

            int mT2 = Nodo("+Mago II", "Otro mago mas por ronda", 9,
                MonedaTipo.Cristal, SkillAccion.MasCantidad, TorreObjetivo.Mago, xM1, Fila(4));
            Req(mT2, m2d); Req(mT2, m2c); Req(mT2, m2r);

            int m3d = Nodo("Dano Mago III", "Mas dano del mago", 70,
                MonedaTipo.Oro, SkillAccion.MasDano, TorreObjetivo.Mago, xM0, Fila(5));
            int m3c = Nodo("Cadencia Mago III", "El mago ataca mas seguido", 70,
                MonedaTipo.Oro, SkillAccion.MasCadencia, TorreObjetivo.Mago, xM2, Fila(5));
            Req(m3d, mT2); Req(m3c, mT2);

            // Mejora definitiva del mago: el haz no se apaga
            int mEsp = Nodo("Haz Continuo", "El haz del mago no se apaga", 150,
                MonedaTipo.Oro, SkillAccion.MejoraEspecial, TorreObjetivo.Mago, xM1, Fila(6));
            Req(mEsp, m3d); Req(mEsp, m3c);

            // ── Rama Castillo (independiente, lineal) ──────────────────────────
            const int xC = 685;

            int c1 = Nodo("Vida Castillo", "+1 de vida al castillo", 4,
                MonedaTipo.Cristal, SkillAccion.VidaCastillo, TorreObjetivo.Castillo, xC, Fila(1));
            int c2 = Nodo("Vida Castillo II", "+1 de vida al castillo", 6,
                MonedaTipo.Cristal, SkillAccion.VidaCastillo, TorreObjetivo.Castillo, xC, Fila(2));
            int c3 = Nodo("Vida Castillo III", "+1 de vida al castillo", 8,
                MonedaTipo.Cristal, SkillAccion.VidaCastillo, TorreObjetivo.Castillo, xC, Fila(3));
            Req(c2, c1); Req(c3, c2);
        }

        // Agrega un requisito (nodo que debe estar comprado) a un nodo.
        private void Req(int nodo, int requisito) => Nodos[nodo].Requisitos.Add(requisito);

        // Marca como desbloqueado todo nodo cuyos requisitos esten todos comprados.
        // Un nodo sin requisitos queda desbloqueado de entrada.
        private void ActualizarDesbloqueos()
        {
            foreach (SkillNode n in Nodos)
                n.Desbloqueado = n.Requisitos.All(r => Nodos[r].Comprado);
        }

        // Intenta comprar un nodo. Devuelve true si la compra se concreto.
        public bool Comprar(int indice)
        {
            SkillNode nodo = Nodos[indice];
            if (nodo.Comprado || !nodo.Desbloqueado)
                return false;

            GameManager gm = GameManager.Instance;

            // Cobrar con la moneda correspondiente
            if (nodo.Moneda == MonedaTipo.Oro)
            {
                if (gm.Monedas < nodo.Costo) return false;
                gm.SpendCoins(nodo.Costo);
            }
            else
            {
                if (gm.Cristales < nodo.Costo) return false;
                gm.SpendCristales(nodo.Costo);
            }

            nodo.Comprado = true;
            AplicarAccion(nodo);
            ActualizarDesbloqueos();
            return true;
        }

        private void AplicarAccion(SkillNode n)
        {
            GameManager gm = GameManager.Instance;
            switch (n.Accion)
            {
                case SkillAccion.DesbloquearTorre: gm.DesbloquearTorre(n.Torre);  break;
                case SkillAccion.MasCantidad:      gm.SumarCantidad(n.Torre);     break;
                case SkillAccion.MasDano:          gm.SubirDano(n.Torre);         break;
                case SkillAccion.MasRango:         gm.SubirRango(n.Torre);        break;
                case SkillAccion.MasCadencia:      gm.SubirCadencia(n.Torre);     break;
                case SkillAccion.MasRalentizacion: gm.SubirRalentizacion(n.Torre); break;
                case SkillAccion.MejoraEspecial:   gm.ActivarEspecial(n.Torre);   break;
                case SkillAccion.VidaCastillo:     gm.SubirVidaCastillo();        break;
            }
        }
    }
}
