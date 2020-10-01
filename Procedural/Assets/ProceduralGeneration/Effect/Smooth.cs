using System;

namespace ProceduralGeneration.Effect
{
    public class Smooth : ILandMapEffect
    {
        private readonly int amount;

        public Smooth(int amount)
        {
            this.amount = amount;
        }

        void ILandMapEffect.Effect(LandMap landMap)
        {
            if(amount <= 1)
                return;
            
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var nextValue = landMap.GetHeight(x, y);
                var nextValueCount = 0;

                for (var xRange = Math.Max(x - amount, 0); xRange < Math.Min(x + amount, LandMap.Size); xRange++)
                for (var yRange = Math.Max(y - amount, 0); yRange < Math.Min(y + amount, LandMap.Size); yRange++, nextValueCount++)
                {
                    var subX = x - xRange;
                    var subY = y - yRange;
                    
                    if (!(Math.Sqrt(subX * subX + subY * subY) <= amount))
                        continue;

                    nextValue += landMap.GetHeight(xRange, yRange);
                }

                var finalValue = nextValue / nextValueCount;
                landMap.SetHeight(x, y, finalValue);
            }
        }
    }
}