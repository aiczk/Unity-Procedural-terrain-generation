using ProceduralGeneration.Effect;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralGeneration
{
    public class LandMap
    {
        //warning: do not change! 
        internal const int TextureSize = 500;
        internal const int Size = 257;
        
        private const int Max = 256;
        private float[] map;

        public LandMap()
        {
            map = new float[Size * Size];
        }

        private static float RandomValue => Random.Range(0.1f, 1.0f);

        internal float GetHeight(int x, int y)
        {
            if (x < 0 || x > Max || y < 0 || y > Max)
                return -1;

            var index = x + Size * y;
            return map[index];
        }

        internal void SetHeight(int x, int y, float value)
        {
            var index = x + Size * y;
            map[index] = value;
        }

        public LandMap Initialize(float deviation)
        {
            var random = RandomValue + Max;

            SetHeight(0, 0, random);
            SetHeight(Max, 0, random);
            SetHeight(Max, Max, random);
            SetHeight(0, Max, random);
    
            Subdivide(Max, deviation);

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
                
                for (y = half; y < Max; y += size)
                for (x = half; x < Max; x += size)
                    Square(x, y, half, RandomValue * scale * 2 - scale);

                for (y = 0; y <= Max; y += half)
                for (x = (y + half) % size; x <= Max; x += size)
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

            var ave = Average
            (
                GetHeight(subX, subY),
                GetHeight(addX, subY),
                GetHeight(addX, addY),
                GetHeight(subX, addY)
            );
    
            SetHeight(x, y, ave + offset);
        }

        private void Diamond(int x, int y, int size, float offset)
        {
            var addX = x + size;
            var subX = x - size;
            var addY = y + size;
            var subY = y - size;
                
            var ave = Average
            (
                GetHeight(x, subY),
                GetHeight(addX, y),
                GetHeight(x, addY),
                GetHeight(subX, y)
            );
    
            SetHeight(x, y, ave + offset);
        }

        private float Average(params float[] values)
        {
            var average = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                ref var value = ref values[i];
                
                if(value.Equals(-1f))
                    continue;

                average += value;
            }
            
            return average / values.Length;
        }

        //-----

        public LandMap AddEffect(ILandMapEffect lmEffect)
        {
            lmEffect.Effect(this);
            return this;
        }
        
        public Mesh CreateMesh(LMHeightMap lmHeightMap, float height = 100, int landMapSize = 100)
        {
            var heightMap = lmHeightMap.HeightMap;
            
            if (TextureSize != landMapSize) 
                heightMap = LandMapExtension.Resize(heightMap, landMapSize);

            var halfSize = Mathf.CeilToInt((float) landMapSize / 2);
            var verticesSize = halfSize * halfSize;
            
            var triangles = new int[(verticesSize - (landMapSize - 1)) * 6];
            var vertices = new Vector3[verticesSize];
            var uv = new Vector2[verticesSize];
            
            var vert = 0;
            var tri = 0;
            
            for (var i = 0; i < halfSize; i++)
            for (var j = 0; j < halfSize; j++)
            {
                vertices[vert] = new Vector3(i, heightMap.GetPixel(i, j).grayscale * height / 3, j);
                ++vert;

                if (i == 0 || j == 0)
                    continue;

                var addIj = halfSize * i + j;
                var subIj = halfSize * (i - 1) + j;

                Increment(addIj);
                Increment(addIj - 1);
                Increment(subIj - 1);
                Increment(subIj - 1);
                Increment(subIj);
                Increment(addIj);
            }

            for (var i = 0; i < verticesSize; i++)
                uv[i] = new Vector2(vertices[i].x, vertices[i].z);
            
            var procMesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv
            };
            
            procMesh.RecalculateNormals();
            
            return procMesh;
            
            void Increment(int value)
            {
                triangles[tri] = value;
                tri++;
            }
        }
    }
}