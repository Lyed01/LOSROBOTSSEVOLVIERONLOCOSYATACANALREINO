using System;

namespace ProyectoSDL2.Game
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y) { X = x; Y = y; }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float s)   => new Vector2(a.X * s, a.Y * s);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public Vector2 Normalized()
        {
            float len = Length;
            if (len < 0.0001f) return new Vector2(0, 0);
            return new Vector2(X / len, Y / len);
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
