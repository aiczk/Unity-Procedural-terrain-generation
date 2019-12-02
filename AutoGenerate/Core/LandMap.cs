using System;
using System.Globalization;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable ParameterHidesMember
// ReSharper disable once CheckNamespace

namespace Procedural
{
    public class LandMap
    {
        private const int TextureSize = 500;

        private float[] map;
        private int size = 257;
        private int max = 256;
        
        public LandMap()
        {
        }

        private static float RandomValue() => Random.Range(0.1f, 1.0f);

        private float Get(int x, int y)
        {
            if (x < 0 || x > max || y < 0 || y > max)
                return -1;

            var index = x + size * y;
            return map[(int) index];
        }

        private void Set(int x, int y, float value)
        {
            var index = x + size * y;
            map[(int) index] = value;
        }

        public void SetUp(float deviation)
        {
            if (map == null)
            {
                var arraySize = size * size;
                map = new float[arraySize];
            }

            var randomValue = RandomValue() + max;

            Set(0, 0, randomValue);
            Set(max, 0, randomValue);
            Set(max, max, randomValue);
            Set(0, max, randomValue);
    
            Subdivide(max, deviation);        
        }
        
        #region SetUp

        private void Subdivide(int size, float deviation)
        {
            while (true)
            {
                int x, y;
                var half = size / 2;
                var scale = deviation * size;

                if (half < 1) 
                    break;
                
                for (y = half; y < max; y += size)
                for (x = half; x < max; x += size)
                    Square(x, y, half, RandomValue() * scale * 2 - scale);

                for (y = 0; y <= max; y += half)
                for (x = (y + half) % size; x <= max; x += size)
                    Diamond(x, y, half, RandomValue() * scale * 2 - scale);

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
                Get(subX, subY),
                Get(addX, subY),
                Get(addX, addY),
                Get(subX, addY)
            );
    
            Set(x, y, ave + offset);
        }

        private void Diamond(int x, int y, int size, float offset)
        {
            var addX = x + size;
            var subX = x - size;
            var addY = y + size;
            var subY = y - size;
                
            var ave = Average
            (
                Get(x, subY),
                Get(addX, y),
                Get(x, addY),
                Get(subX, y)
            );
    
            Set(x, y, ave + offset);
        }
        
        private float Average(params float[] values)
        {
            var count = 0;
            var average = 0f;
            for (var i = 0; i < values.Length; i++)
            {
                ref var value = ref values[i];
                
                if(value.Equals(-1f))
                    continue;

                average += value;
                count++;
            }
            
            return average / count;
        }
        
        #endregion
        
        public void Smoothness(int amount)
        {
            if(amount <= 1)
                return;
            
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nextValue = Get(x, y);
                var nextValueCount = 0;

                for (var xRange = Math.Max(x - amount, 0); xRange < Math.Min(x + amount, size); xRange++)
                for (var yRange = Math.Max(y - amount, 0); yRange < Math.Min(y + amount, size); yRange++)
                {
                    var subX = x - xRange;
                    var subY = y - yRange;
                    
                    if (!(Math.Sqrt(subX * subX + subY * subY) <= amount))
                        continue;

                    nextValue += Get(xRange, yRange);
                    nextValueCount++;
                }

                var finalValue = nextValue / nextValueCount;
                Set(x, y, finalValue);
            }
        }
        
        public void Combine(LandMap to)
        {
            var maxValue = 0f;
            var minValue = 0f;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var val = to.Get(x, y);

                maxValue = Math.Max(maxValue, val);
                minValue = Math.Min(minValue, val);
            }
            
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var featureOne = Get(x, y);
                var featureTwo = to.Get(x, y);

                var percent = featureTwo / Math.Abs(maxValue - minValue);
                var value = (1 - percent) * featureOne + percent * featureTwo;
                
                Set(x, y, value);
            }
        }

        #region Texture
        
        public Texture2D Perlin(float noiseScale,float rounding = 0.5f, TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            var texture = LandMapExtension.CreateTexture(TextureSize, "ProceduralPerlin");
            
            for (var y = 0; y < TextureSize; y++)
            for (var x = 0; x < TextureSize; x++)
            {
                var xSamp = random + (float) x / TextureSize * noiseScale;
                var ySamp = random + (float) y / TextureSize * noiseScale;

                var color = Mathf.PerlinNoise(xSamp, ySamp);

                if (color <= rounding)
                    color = 0;

                texture.SetPixel(x, y, new Color(color, color, color, color));
            }

            texture.Apply();
    
            return texture;
        }
        
        public Texture2D OctavePerlin(float noiseScale,float noise = 0.4f,TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            var texture = LandMapExtension.CreateTexture(TextureSize, "ProceduralPerlin");
            
            for (var y = 0; y < TextureSize; y++)
            for (var x = 0; x < TextureSize; x++)
            {
                var xSample = random + (float) x / TextureSize * noiseScale;
                var ySample = random + (float) y / TextureSize * noiseScale;

                var color = OctavePerlinNoise(xSample, ySample, noise);

                texture.SetPixel(x, y, new Color(color, color, color, color));
            }

            texture.Apply();
    
            return texture;
        }
        
        private float OctavePerlinNoise(float x, float y, float noise = 0.4f)
        {
            var a = noise;
            var f = noise;
            var maxValue = 0.0f;
            var total = 0.0f;
            var per = 0.5f;
    
            for (var i = 0; i < 5; ++i)
            {
                total += a * Mathf.PerlinNoise(x * f, y * f);
                maxValue += a;
                a *= per;
                f *= 2.0f;
            }
    
            return total / maxValue;
        }
        
        #endregion
    
        public Texture2D HeightMap(TextureFormat format = TextureFormat.RGB24)
        {
            var texture = LandMapExtension.CreateTexture(TextureSize, "ProceduralHeightMap", FilterMode.Bilinear, format);
    
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                texture.SetPixel(x, y, Brightness(TextureSize, Get(x, y)));

            texture.Apply();

            return texture;
        }
        
        #region HeightMap
        
        private Color Brightness(int textureSize, float value)
        {
            var val = Mathf.Floor(value / textureSize);
            val = Mathf.Clamp01(val);
            val = Mathf.Lerp(val, 1, value / textureSize);
            
            return new Color(val, val, val, val);
        }
        
        #endregion

        public Mesh CreateMesh(Texture2D heightMap, float height = 100, int landMapSize = 100)
        {
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