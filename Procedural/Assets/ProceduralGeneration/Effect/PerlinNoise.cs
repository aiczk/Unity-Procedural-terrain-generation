using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class PerlinNoise : ILandMapEffect
    {
        private readonly float noiseScale, rounding;

        public PerlinNoise(float noiseScale, float rounding = 0.5f)
        {
            this.noiseScale = noiseScale;
            this.rounding = rounding;
        }
        
        void ILandMapEffect.Effect(LandMap landMap)
        {
            var random = Random.Range(-1000f, 1000f);
            
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var xSample = random + (float) x / LandMap.Size * noiseScale;
                var ySample = random + (float) y / LandMap.Size * noiseScale;

                var noise = Mathf.PerlinNoise(xSample, ySample);

                if (noise <= rounding)
                    noise = 0;

                var value = landMap.GetHeight(x, y);
                landMap.SetHeight(x, y, noise + value);
            }
        }
    }
}