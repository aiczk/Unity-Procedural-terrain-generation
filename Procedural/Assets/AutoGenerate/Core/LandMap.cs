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
        //warn: do not change! 
        private const int TextureSize = 500;

        private float[] map;
        private static readonly int Size = 257;
        private static readonly int Max = 256;

        public LandMap()
        {
            map = new float[Size * Size];
        }

        private static float RandomValue => Random.Range(0.1f, 1.0f);

        private float GetHeight(int x, int y)
        {
            if (x < 0 || x > Max || y < 0 || y > Max)
                return -1;

            var index = x + Size * y;
            return map[index];
        }

        private void SetHeight(int x, int y, float value)
        {
            var index = x + Size * y;
            map[index] = value;
        }

        public void Initialize(float deviation)
        {
            var random = RandomValue + Max;

            SetHeight(0, 0, random);
            SetHeight(Max, 0, random);
            SetHeight(Max, Max, random);
            SetHeight(0, Max, random);
    
            Subdivide(Max, deviation);      
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

        public void Smoothness(int amount)
        {
            if(amount <= 1)
                return;
            
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var nextValue = GetHeight(x, y);
                var nextValueCount = 0;

                for (var xRange = Math.Max(x - amount, 0); xRange < Math.Min(x + amount, Size); xRange++)
                for (var yRange = Math.Max(y - amount, 0); yRange < Math.Min(y + amount, Size); yRange++)
                {
                    var subX = x - xRange;
                    var subY = y - yRange;
                    
                    if (!(Math.Sqrt(subX * subX + subY * subY) <= amount))
                        continue;

                    nextValue += GetHeight(xRange, yRange);
                    nextValueCount++;
                }

                var finalValue = nextValue / nextValueCount;
                SetHeight(x, y, finalValue);
            }
        }

        public void Combine(LandMap other)
        {
            var maxValue = 0f;
            var minValue = 0f;

            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var val = other.GetHeight(x, y);

                maxValue = Math.Max(maxValue, val);
                minValue = Math.Min(minValue, val);
            }
            
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var featureOne = GetHeight(x, y);
                var featureTwo = other.GetHeight(x, y);

                var percent = featureTwo / Math.Abs(maxValue - minValue);
                var value = (1 - percent) * featureOne + percent * featureTwo;
                
                SetHeight(x, y, value);
            }
        }

        public Texture2D Perlin(float noiseScale, float rounding = 0.5f, TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            var texture = LandMapExtension.CreateTexture(TextureSize, "ProceduralPerlin");
            
            for (var y = 0; y < TextureSize; y++)
            for (var x = 0; x < TextureSize; x++)
            {
                var xSample = random + (float) x / TextureSize * noiseScale;
                var ySample = random + (float) y / TextureSize * noiseScale;

                var color = Mathf.PerlinNoise(xSample, ySample);

                if (color <= rounding)
                    color = 0;

                texture.SetPixel(x, y, new Color(color, color, color, color));
            }

            texture.Apply();
    
            return texture;
        }

        public Texture2D OctavePerlin(float noiseScale, float noise = 0.4f, TextureFormat format = TextureFormat.RGBA32)
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
    
            return texture;        }

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

        //-----

        public Texture2D HeightMap(TextureFormat format = TextureFormat.RGB24)
        {
            var texture = LandMapExtension.CreateTexture(TextureSize, "ProceduralHeightMap", FilterMode.Bilinear, format);
    
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
                texture.SetPixel(x, y, Brightness(TextureSize, GetHeight(x, y)));

            texture.Apply();

            return texture;        }

        private Color Brightness(int textureSize, float value)
        {
            var val = Mathf.Floor(value / textureSize);
            val = Mathf.Clamp01(val);
            val = Mathf.Lerp(val, 1, value / textureSize);
            
            return new Color(val, val, val, val);
        }

        //-----

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

        public ILandMapEffect Add(ILandMapEffect effect)
        {
            return effect;
        }
    }

    public interface ILandMapEffect
    {
        ILandMapEffect Add(ILandMapEffect effect);
    }

    public class Perlin : ILandMapEffect
    {
        ILandMapEffect ILandMapEffect.Add(ILandMapEffect effect)
        {
            
            return effect;
        }
    }
}