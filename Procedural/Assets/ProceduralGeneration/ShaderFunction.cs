using UnityEngine;

namespace ProceduralGeneration
{
    public static class ShaderFunction
    {
        public static float Fract(float x) => x - Mathf.Floor(x);
        public static Vector2 Fract(Vector2 vec) => new Vector2(Fract(vec.x), Fract(vec.y));
        public static Vector2 Floor(Vector2 vec) => new Vector2(Mathf.Floor(vec.x), Mathf.Floor(vec.y));
    }
}