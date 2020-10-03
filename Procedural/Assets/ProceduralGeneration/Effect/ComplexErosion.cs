using System.Linq;
using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class ComplexErosion : ILandMapEffector
    {
        private readonly float carryingCapacity, depositionSpeed;
        private readonly int iterations, drops;

        public ComplexErosion(float carryingCapacity, float depositionSpeed, int iterations, int drops)
        {
            this.carryingCapacity = carryingCapacity;
            this.depositionSpeed = depositionSpeed;
            this.iterations = iterations;
            this.drops = drops;
        }

        void ILandMapEffector.Effect(LandMap landMap)
        {
            for (var drop = 0; drop < drops; drop++)
            {
                var x = Mathf.FloorToInt(LandMap.RandomValue * LandMap.Size);
                var y = Mathf.FloorToInt(LandMap.RandomValue * LandMap.Size);
                Deposit(landMap, x, y, depositionSpeed, carryingCapacity);
            }
        }
        
        private void Deposit(LandMap landMap, int x, int y, float kd, float kq)
        {
            var c = 0f;
            var v = 1.05f;
            const float minSlope = 1.15f;
            const float maxVelocity = 10f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                v = Mathf.Min(v, maxVelocity);
                var value = landMap.GetHeight(x, y);

                float[] nv = 
                {
                    landMap.GetHeight(x, y - 1), //North
                    landMap.GetHeight(x, y + 1), //South
                    landMap.GetHeight(x + 1, y), //East
                    landMap.GetHeight(x - 1, y)  //West
                };

                var minInd = IndexOfMinimum(nv);

                if (!(nv[minInd] < value)) 
                    continue;

                var slope = Mathf.Min(minSlope, value - nv[minInd]);
                var vtc = kd * v * slope;

                if (c > kq)
                {
                    c -= vtc;
                    landMap.SetHeight(x, y, landMap.GetHeight(x, y) + vtc);
                }
                else
                {
                    if (c + vtc > kq)
                    {
                        var delta = c + vtc - kq;
                        c += delta;
                        landMap.SetHeight(x, y, landMap.GetHeight(x, y) + delta);
                    }
                    else
                    {
                        c += vtc;
                        landMap.SetHeight(x, y, landMap.GetHeight(x, y) - vtc);
                    }
                }

                switch (minInd)
                {
                    case 0: y -= 1; break;
                    case 1: y += 1; break;
                    case 2: x += 1; break;
                    case 3: x -= 1; break;
                }

                if (x > LandMap.MaxSize) 
                    x = LandMap.Size;

                if (y > LandMap.MaxSize) 
                    y = LandMap.Size;

                if (x < 0) 
                    x = 0;

                if (y < 0) 
                    y = 0;
            }
        }

        private int IndexOfMinimum(float[] nv)
        {
            var index = 0;
            var min = nv[0];
            
            for (var i = 0; i < 3; i++)
            {
                if (!(nv[i] < min))
                    continue;
                
                min = nv[i];
                index = i;
            }
            
            return index;
        }
    }
}