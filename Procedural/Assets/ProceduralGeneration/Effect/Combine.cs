using System;

namespace ProceduralGeneration.Effect
{
    public class Combine : ILandMapEffect
    {
        private readonly LandMap other;

        public Combine(LandMap other)
        {
            this.other = other;
        }

        void ILandMapEffect.Effect(LandMap landMap)
        {
            var maxValue = 0f;
            var minValue = 0f;

            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var val = other.GetHeight(x, y);

                maxValue = Math.Max(maxValue, val);
                minValue = Math.Min(minValue, val);
            }
            
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var featureOne = landMap.GetHeight(x, y);
                var featureTwo = other.GetHeight(x, y);

                var percent = featureTwo / Math.Abs(maxValue - minValue);
                var value = (1 - percent) * featureOne + percent * featureTwo;
                
                landMap.SetHeight(x, y, value);
            }
        }
    }
}