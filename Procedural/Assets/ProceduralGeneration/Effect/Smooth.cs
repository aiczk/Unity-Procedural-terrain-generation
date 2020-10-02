using System;

namespace ProceduralGeneration.Effect
{
    public class Smooth : ILandMapEffector
    {
        private readonly int level;

        public Smooth(int level)
        {
            this.level = level;
        }

        void ILandMapEffector.Effect(LandMap landMap)
        {
            if(level <= 1)
                return;
            
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
            {
                var nextValue = landMap.GetHeight(x, y);
                var nextValueCount = 0;

                for (var xRange = Math.Max(x - level, 0); xRange < Math.Min(x + level, LandMap.Size); xRange++)
                for (var yRange = Math.Max(y - level, 0); yRange < Math.Min(y + level, LandMap.Size); yRange++, nextValueCount++)
                {
                    var subX = x - xRange;
                    var subY = y - yRange;
                    
                    if (!(Math.Sqrt(subX * subX + subY * subY) <= level))
                        continue;

                    nextValue += landMap.GetHeight(xRange, yRange);
                }

                var finalValue = nextValue / nextValueCount;
                landMap.SetHeight(x, y, finalValue);
            }
        }
    }
}