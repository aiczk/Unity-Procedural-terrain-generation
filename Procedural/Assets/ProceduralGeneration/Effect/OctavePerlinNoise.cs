using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class OctavePerlinNoise : ILandMapEffector
    {
        private readonly float noiseScale, noise;

        public OctavePerlinNoise(float noiseScale, float noise = 0.4f)
        {
            this.noiseScale = noiseScale;
            this.noise = noise;
        }

        void ILandMapEffector.Effect(LandMap landMap)
        {
            var random = Random.Range(-1000f, 1000f);
            
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var xSample = random + (float) x / LandMap.Size * noiseScale;
                var ySample = random + (float) y / LandMap.Size * noiseScale;

                var noiseValue = OctavePerlin(xSample, ySample);
                var value = landMap.GetHeight(x, y);
                landMap.SetHeight(x, y, noiseValue + value);
            }
        }
        
        private float OctavePerlin(float x, float y)
        {
            var a = noise;
            var f = noise;
            var maxValue = 0.0f;
            var total = 0.0f;
            const float per = 0.5f;
    
            for (var i = 0; i < 5; ++i)
            {
                total += a * Mathf.PerlinNoise(x * f, y * f);
                maxValue += a;
                a *= per;
                f *= 2.0f;
            }
    
            return total / maxValue;
        }

    }
}