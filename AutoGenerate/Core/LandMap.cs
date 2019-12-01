using System;
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
        private float size, max;
        
        public LandMap()
        {
            size = 257;
            max = size - 1;
        }

        private static float RandomValue() => Random.Range(0.1f, 1.0f);
        
        private float Get(float x,float y)
        {        
            if (x < 0 || x > max || y < 0 || y > max)
                return -1;

            var index = x + size * y;
            return map[(int) index];
        }
        
        private void Set(float x, float y, float value)
        {
            var index = x + size * y;
            map[(int) index] = value;
        }
    
        public void SetUp(float deviation)
        {
            if (map == null)
            {
                var arraySize = size * size;
                map = new float[(int)arraySize];
            }

            var randomValue = RandomValue() + max;

            Set(0, 0, randomValue);
            Set(max, 0, randomValue);
            Set(max, max, randomValue);
            Set(0, max, randomValue);
    
            Subdivide(max, deviation);        
        }
        
        #region Initialize

        private void Subdivide(float size, float deviation)
        {
            while (true)
            {
                float x, y;
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
        
        private void Square(float x, float y, float size, float offset)
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

        private void Diamond(float x, float y, float size, float offset)
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

            var roundAmount = Mathf.Round(amount);
            
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nextValue = Get(x, y);
                var nextValueCount = 0;

                for (var xRange = Math.Max(x - roundAmount, 0); xRange < Math.Min(x + roundAmount, size); xRange++)
                for (var yRange = Math.Max(y - roundAmount, 0); yRange < Math.Min(y + roundAmount, size); yRange++)
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
        
        #region Texture
        
        public Texture2D Perlin(float noiseScale,float rounding = 0.5f, TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            var texture = CreateTexture("ProceduralPerlin");
            
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
            var texture = CreateTexture("ProceduralPerlin");
            
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
        public Texture2D Resize(Texture2D texture, int size)
        {
            var rt = RenderTexture.GetTemporary(size, size);
            var preRt = RenderTexture.active;
            
            RenderTexture.active = rt;
            
            Graphics.Blit(texture, rt);
            var ret = new Texture2D(size, size);
            ret.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            ret.Apply();
            
            RenderTexture.active = preRt;
            RenderTexture.ReleaseTemporary(rt);
            
            return ret;
        }
        public Texture2D MergeTexture(Texture2D background, Texture2D overlay)
        {
            const int startX = 0;
            var startY = background.height - overlay.height;
    
            for (var x = startX; x < background.width; x++)
            for (var y = startY; y < background.height; y++)
            {
                var bgColor = background.GetPixel(x, y);
                var olColor = overlay.GetPixel(x, y);

                var finalColor = Color.Lerp(bgColor, olColor, olColor.a / 1.0f);

                background.SetPixel(x, y, finalColor);
            }

            background.Apply();
            return background;
        }
        private Texture2D CreateTexture(string name, FilterMode filterMode = FilterMode.Point, TextureFormat format = TextureFormat.RGBA32)
        {
            return new Texture2D(TextureSize, TextureSize, format, false)
            {
                filterMode = filterMode,
                name = name
            };
        }
        
        private float OctavePerlinNoise(float x, float y,float noise = 0.4f)
        {
            var a = noise;
            var f = noise;
            var total = 0.0f;
            var per = 0.5f;
    
            for (var i = 0; i < 5; ++i)
            {
                total += a * Mathf.PerlinNoise(x * f, y * f);
                max += a;
                a *= per;
                f *= 2.0f;
            }
    
            return total / max;
        }
        
        #endregion
    
        public Texture2D HeightMap(TextureFormat format = TextureFormat.RGB24)
        {
            var texture = CreateTexture("ProceduralHeightMap", FilterMode.Bilinear, format);
    
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

        public Mesh CreateMesh(Texture2D heightMap, float height = 100, int size = 100)
        {
            if (size != TextureSize) 
                heightMap = Resize(heightMap, size);

            var terrainSize = size / 2;
            var arraySize = terrainSize * terrainSize;
            var vertices = new Vector3[arraySize];
            var triangles = new int[arraySize * 6 - (size - 1) * 6];
            var uv = new Vector2[arraySize];
            
            var vert = 0;
            var tri = 0;
            
            for (var i = 0; i < terrainSize; i++)
            for (var j = 0; j < terrainSize; j++)
            {
                vertices[vert] = new Vector3(i, heightMap.GetPixel(i, j).grayscale * height / 3, j);
                vert++;

                if (i == 0 || j == 0)
                    continue;

                var addIj = terrainSize * i + j;
                var subIj = terrainSize * (i - 1) + j;

                Increment(addIj);
                Increment(addIj - 1);
                Increment(subIj - 1);
                Increment(subIj - 1);
                Increment(subIj);
                Increment(addIj);
            }

            for (var i = 0; i < arraySize; i++)
                uv[i] = new Vector2(vertices[i].x, vertices[i].z);

            var procMesh = new Mesh
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles
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