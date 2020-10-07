using System.Globalization;
using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class VoroNoise : ILandMapEffector
    {
        private readonly float u, v;

        public VoroNoise(float u = 1, float v = 1)
        {
            this.u = u;
            this.v = v;
        }
        
        void ILandMapEffector.Effect(LandMap landMap)
        {
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var voronoi = Voronoi(x, y, u, v);
                var value = landMap.GetHeight(x, y);
                landMap.SetHeight(x, y, voronoi + value);
            }
        }

        private static float Voronoi(int x, int y, float u, float v)
        {
            var vec = new Vector2(x, y);
            var p = ShaderFunction.Floor(vec);
            var f = ShaderFunction.Fract(vec);
            var k = 1f + 63f * Mathf.Pow(1f - v, 6f);
            var a = Vector2.zero;
            
            for (var i = -2; i <= 2; i++)
            for (var j = -2; j <= 2; j++)
            {
                var g = new Vector2(i, j);
                var o = Vector3.Scale(ShaderFunction.Hash3(p + g), new Vector3(u, u, 1f));
                var d = g - f + new Vector2(o.x, o.y);
                var w = Mathf.Pow(1f - Mathf.SmoothStep(0f, 1.414f, ShaderFunction.Length(d)), k);
                a += new Vector2(o.z * w, w);
            }

            return a.x / a.y;
        }
    }
}