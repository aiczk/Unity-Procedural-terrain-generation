using System;

namespace ProceduralGeneration.Effect
{
    public class Combine : ILandMapEffector
    {
        private readonly LandMap other;

        public Combine(LandMap other) => this.other = other;

        void ILandMapEffector.Effect(LandMap landMap)
        {
            var maxValue = 0f;
            var minValue = 0f;

            //todo: use linq min/max
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var value = other.GetHeight(x, y);

                maxValue = Math.Max(maxValue, value);
                minValue = Math.Min(minValue, value);
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