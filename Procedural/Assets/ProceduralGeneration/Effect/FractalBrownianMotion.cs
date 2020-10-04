using System.Globalization;
using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class FractalBrownianMotion : ILandMapEffector
    {
        private readonly int octaves;
        private readonly float lacunarity, gain;

        public FractalBrownianMotion(int octaves = 6, float lacunarity = 2f, float gain = 0.5f)
        {
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.gain = gain;
        }
        
        void ILandMapEffector.Effect(LandMap landMap)
        {
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var fbm = fBM(x, y, octaves, lacunarity, gain);
                var value = landMap.GetHeight(x, y);
                landMap.SetHeight(x, y, fbm + value);
            }
        }
        
        private static float Noise(Vector2 st)
        {
            var i = ShaderFunction.Floor(st);
            var f = ShaderFunction.Fract(st);

            var a = Random(i);
            var b = Random(i + new Vector2(1, 0));
            var c = Random(i + new Vector2(0, 1));
            var d = Random(i + new Vector2(1, 1));

            var u = f * f * Subtract(2.0f * f, 3.0f);

            return Mathf.Lerp(a, b, u.x) + (c - a) * u.y * (1.0f - u.x) + (d - b) * u.x * u.y;
            
            float Random(Vector2 vec) => ShaderFunction.Fract(Mathf.Sin(Vector2.Dot(vec, new Vector2(12.9898f, 78.233f)) * 43758.5453123f));
            Vector2 Subtract(Vector2 vec, float v) => new Vector2(vec.x - v, vec.y - v);
        }

        // ReSharper disable once InconsistentNaming
        private static float fBM(int x, int y, int octaves, float lacunarity, float gain)
        {
            float value = 0f, amplitude = 0.5f;
            var st = new Vector2(x, y);

            for (var i = 0; i < octaves; i++)
            {
                value += amplitude * Noise(st);
                st *= lacunarity;
                amplitude *= gain;
            }

            return value;
        }
    }
}