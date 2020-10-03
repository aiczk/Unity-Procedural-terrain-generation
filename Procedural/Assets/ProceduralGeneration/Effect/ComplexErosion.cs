using System.Linq;
using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class ComplexErosion : ILandMapEffector
    {
        private float carryingCapacity;
        private float depositionSpeed;
        private int iterations;
        private int drops;
        private float[] heightMap;
        private int size;

        public ComplexErosion(float carryingCapacity, float depositionSpeed, int iterations, int drops)
        {
            this.carryingCapacity = carryingCapacity;
            this.depositionSpeed = depositionSpeed;
            this.iterations = iterations;
            this.drops = drops;
        }

        void ILandMapEffector.Effect(LandMap landMap)
        {
            size = LandMap.MaxSize;
            heightMap = new float[LandMap.Size * LandMap.Size];

            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
                heightMap[GetIndex(x, y)] = landMap.GetHeight(x, y);

            for (var drop = 0; drop < drops; drop++)
            {
                var x = Mathf.FloorToInt(LandMap.RandomValue * LandMap.Size);
                var y = Mathf.FloorToInt(LandMap.RandomValue * LandMap.Size);
                DepositAt(x, y, depositionSpeed, carryingCapacity);
            }

            landMap.Replace(ref heightMap);
        }
        
        private int GetIndex(int x, int y) => x + size * y;
        private float GetValue(int x, int y) => heightMap[GetIndex(x, y)];

        private void DepositAt(int x, int y, float kd, float kq)
        {
            var c = 0f;
            var v = 1.05f;
            const float minSlope = 1.15f;
            const float maxVelocity = 10f;

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                v = Mathf.Min(v, maxVelocity);
                var value = GetValue(x, y);

                float[] nv = 
                {
                    GetValue(x, y - 1), //North
                    GetValue(x, y + 1), //South
                    GetValue(x + 1, y), //East
                    GetValue(x - 1, y)  //West
                };

                var minInd = IndexOfMin(nv);

                if (!(nv[minInd] < value)) 
                    continue;

                var slope = Mathf.Min(minSlope, value - nv[minInd]);
                var vtc = kd * v * slope;

                if (c > kq)
                {
                    c -= vtc;
                    heightMap[GetIndex(x, y)] += vtc;
                }
                else
                {
                    if (c + vtc > kq)
                    {
                        var delta = c + vtc - kq;
                        c += delta;
                        heightMap[GetIndex(x, y)] -= delta;
                    }
                    else
                    {
                        c += vtc;
                        heightMap[GetIndex(x, y)] -= vtc;
                    }
                }

                switch (minInd)
                {
                    case 0: y -= 1; break;
                    case 1: y += 1; break;
                    case 2: x += 1; break;
                    case 3: x -= 1; break;
                }

                if (x > size - 1) 
                    x = size;

                if (y > size - 1) 
                    y = size;

                if (x < 0) 
                    x = 0;

                if (y < 0) 
                    y = 0;
            }
        }
        
        private int IndexOfMin(float[] arr) => arr.Select((n, i) => new {index = i, value = n}).OrderBy(item => item.value).First().index;
    }
}