using System;
using UnityEngine;

namespace ProceduralGeneration
{
    public static class ShaderFunction
    {
        private static float Sin(float f) => (float) Math.Sin(f);
        private static float Sqrt(float f) => (float) Math.Sqrt(f);
        
        public static float Fract(float x) => x - Mathf.Floor(x);
        public static Vector2 Fract(Vector2 vec) => new Vector2(Fract(vec.x), Fract(vec.y));
        public static Vector2 Floor(Vector2 vec) => new Vector2(Mathf.Floor(vec.x), Mathf.Floor(vec.y));
        public static Vector2 Sin(Vector2 vec) => new Vector2(Sin(vec.x), Sin(vec.y));
        public static float Length(float x) => Mathf.Abs(x);
        public static float Length(Vector2 vec) => Sqrt(Vector2.Dot(vec, vec));
        
        public static Vector2 Hash2(Vector2 vec)
        {
            var q = new Vector2(Vector2.Dot(vec, new Vector2(127.1f, 311.7f)), Vector2.Dot(vec, new Vector2(269.5f, 183.3f)));
            return Fract(Sin(q) * 43758.5453f);
        }

        public static Vector3 Hash3(Vector2 vec)
        {
            var q = new Vector3(Vector2.Dot(vec, new Vector2(127.1f, 311.7f)), Vector2.Dot(vec, new Vector2(269.5f, 183.3f)), Vector2.Dot(vec, new Vector2(419.2f, 371.9f)));
            return Fract(Sin(q) * 43758.5453f);
        }
    }
}