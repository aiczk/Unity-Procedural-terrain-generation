using System;
using ProceduralGeneration.Effect;
using UnityEngine;

namespace ProceduralGeneration
{
    public class LandMap
    {
        //warning: do not change! 
        internal const int TextureSize = 500;
        internal const int Size = 257;
        private const int MaxSize = 256;
        
        private readonly float[] map;

        public LandMap() => map = new float[Size * Size];
        
        private static float RandomValue => UnityEngine.Random.Range(0.1f, 1.0f);

        internal float GetHeight(int x, int y)
        {
            if (x < 0 || x > MaxSize || y < 0 || y > MaxSize)
                return -1;

            var index = x + Size * y;
            return map[index];
        }

        internal void SetHeight(int x, int y, float value)
        {
            var index = x + Size * y;
            map[index] = value;
        }

        internal float Query(Func<float[], float> func) => func(map);
        
        public LandMap Initialize(float deviation)
        {
            var random = RandomValue + MaxSize;

            SetHeight(0, 0, random);
            SetHeight(MaxSize, 0, random);
            SetHeight(MaxSize, MaxSize, random);
            SetHeight(0, MaxSize, random);
    
            Subdivide(MaxSize, deviation);

            return this;
        }

        private void Subdivide(int size, float deviation)
        {
            while (true)
            {
                int x, y;
                var half = size / 2;
                var scale = deviation * size;

                if (half < 1) 
                    break;
                
                for (y = half; y < MaxSize; y += size)
                for (x = half; x < MaxSize; x += size)
                    Square(x, y, half, RandomValue * scale * 2 - scale);

                for (y = 0; y <= MaxSize; y += half)
                for (x = (y + half) % size; x <= MaxSize; x += size)
                    Diamond(x, y, half, RandomValue * scale * 2 - scale);

                size /= 2;
            }
        }

        private void Square(int x, int y, int size, float offset)
        {
            var addX = x + size;
            var subX = x - size;
            var addY = y + size;
            var subY = y - size;

            var average = Average
            (
                GetHeight(subX, subY),
                GetHeight(addX, subY),
                GetHeight(addX, addY),
                GetHeight(subX, addY)
            );
    
            SetHeight(x, y, average + offset);
        }

        private void Diamond(int x, int y, int size, float offset)
        {
            var average = Average
            (
                GetHeight(x, y - size),
                GetHeight(x + size, y),
                GetHeight(x, y + size),
                GetHeight(x - size, y)
            );
    
            SetHeight(x, y, average + offset);
        }

        private float Average(float a, float b, float c, float d)
        {
            Check(ref a);
            Check(ref b);
            Check(ref c);
            Check(ref d);

            return (a + b + c + d) / 4;

            void Check(ref float value)
            {
                if (value.Equals(-1f))
                    value = 0;
            }
        }

        public LandMap AddEffect(ILandMapEffector lmEffector)
        {
            lmEffector.Effect(this);
            return this;
        }
        
        public Mesh CreateMesh(LmMesh lmMesh)
        {
            lmMesh.Effect(this);
            return lmMesh.LandMapMesh;
        }
    }
}