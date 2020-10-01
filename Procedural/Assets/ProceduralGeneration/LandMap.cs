using System.Linq;
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

        public LandMap() => map = new float[Size * Size];
        
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

        public LandMap AddEffect(ILandMapEffect lmEffect)
        {
            lmEffect.Effect(this);
            return this;
        }
        
        /*
        //test
        public Mesh CreateMesh(float height = 100, int landMapSize = 100)
        {
            var halfSize = Mathf.CeilToInt((float) landMapSize / 2);
            var verticesSize = halfSize * halfSize;
            
            var triangles = new int[(verticesSize - (landMapSize - 1)) * 6];
            var vertices = new Vector3[verticesSize];
            var uv = new Vector2[verticesSize];
            
            var vert = 0;
            var tri = 0;

            var min = map.Min();
            var max = map.Max();
            
            for (var i = 0; i < halfSize; i++)
            for (var j = 0; j < halfSize; j++, ++vert)
            {
                vertices[vert] = new Vector3(i, Mathf.InverseLerp(min, max, GetHeight(i, j)) / 3, j);

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
        */

        
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
            for (var j = 0; j < halfSize; j++, ++vert)
            {
                vertices[vert] = new Vector3(i, heightMap.GetPixel(i, j).grayscale * height / 3, j);

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
            
            procMesh.RecalculateBounds();
            procMesh.RecalculateTangents();
            procMesh.RecalculateNormals();
            procMesh.Optimize();

            return procMesh;
            
            void Increment(int value)
            {
                triangles[tri] = value;
                tri++;
            }
        }
    }
}