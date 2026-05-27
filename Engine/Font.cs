using SDL2;
using System;

namespace ProyectoSDL2.Engine
{
    public class Font
    {
        public IntPtr pointer { get; private set; }

        public Font(string fileName, short size)
        {
            pointer = SDL_ttf.TTF_OpenFont(fileName, size);
            if (pointer == IntPtr.Zero)
                Engine.ErrorFatal("Fuente inexistente: " + fileName);
        }
    }
}
