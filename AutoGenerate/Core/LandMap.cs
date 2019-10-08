using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable ParameterHidesMember

namespace Auto.LM
{
    public class LandMap
    {
        private Dictionary<string, float[]> map = new Dictionary<string, float[]>();
        private float size,max;
        private int textureSize;
        
        public LandMap(int textureSize)
        {
            var level = 8;
            size = Mathf.Pow(2, level) + 1;
            max = size - 1;
            this.textureSize = textureSize;
        }

        private float RandomValue() => Random.Range(0.1f, 1.0f);
        
        private float Get(string which,float x,float y)
        {        
            if (x < 0 || x > max || y < 0 || y > max)
            {
                return -1;
            }
    
            var index = x + size * y;
            return map[which][(int) index];
        }
        
        private void Set(string which,float x,float y,float value)
        {
            var index = x +  size * y;
            map[which][(int)index] = value;
        }
    
        public void Generate(string feature,float deviation)
        {
            if (!map.ContainsKey(feature))
            {
                var arraySize = size * size;
                map.Add(feature,new float[(int)arraySize]);
            }
    
            Set(feature, 0, 0, RandomValue() + max);
            Set(feature, max, 0, RandomValue() + max);
            Set(feature, max, max, RandomValue() + max);
            Set(feature, 0, max, RandomValue() + max);
    
            Subdivide(feature, max, deviation);        
        }
    
        #region Generate
    
        private void Subdivide(string feature,float size, float deviation)
        {
            float x, y;
            var half = size / 2;
            var scale = deviation * size;
            
            if(half < 1)
                return;
    
            for (y = half; y < max; y += size)
            for (x = half; x < max; x += size)
                Square(feature, x, y, half, RandomValue() * scale * 2 - scale);
    
            for (y = 0; y <= max; y += half)
            for (x = (y + half) % size; x <= max; x += size)
                Diamond(feature, x, y, half, RandomValue() * scale * 2 - scale);
    
            Subdivide(feature,size / 2,deviation);
        }
    
        private float Average(params float[] values)
        {
            var valid = values.Where(x => !x.Equals(-1f)).ToArray();
            var total = valid.Aggregate(0f, (sum, val) => sum + val);
    
            return total / valid.Count();
        }
    
        private void Square(string which,float x,float y,float size,float offset)
        {        
            var ave = Average
                (
                    Get(which, x - size, y - size),
                    Get(which, x + size, y - size),
                    Get(which, x + size, y + size),
                    Get(which, x - size, y + size)
                );
    
            Set(which, x, y, ave + offset);
        }
    
        private void Diamond(string which, float x, float y,float size, float offset)
        {
            var ave = Average
                (
                    Get(which, x, y - size),
                    Get(which, x + size, y),
                    Get(which, x, y + size),
                    Get(which, x - size, y)
                );
    
            Set(which, x, y, ave + offset);
        }
    
        #endregion
    
        public void Smooth(int amount,string from,string to)
        {
            if (!map.ContainsKey(to))
            {
                var arraySize = size * size;
                map.Add(to,new float[(int)arraySize]);
            }
    
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nextValue = Get(from, x, y);
                    var nextValueCount = 0;
    
                    for (var xRange = Mathf.Max(x - Mathf.Round(amount),0); xRange < Mathf.Min(x + Mathf.Round(amount),size); xRange++)
                    {
                        for (var yRange = Mathf.Max(y - Mathf.Round(amount),0); yRange < Mathf.Min(y + Mathf.Round(amount),size); yRange++)
                        {
                            if (!(Mathf.Abs(Mathf.Sqrt((x - xRange) * (x - xRange) + (y - yRange) * (y - yRange))) <= amount))
                                continue;
                            
                            var value = Get(from, xRange, yRange);
                            nextValue += value;
                            nextValueCount++;
                        }
                    }
    
                    var finalValue = nextValue / nextValueCount;
                    Set(to, x, y, finalValue);
                }
            }
        }
        
        public Texture2D DrawPerlin(float noiseScale,float rounding = 0.5f,TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            
            var texture = new Texture2D(textureSize, textureSize, format, false)
            {
                filterMode = FilterMode.Point,
                name = "ProceduralPerlin"
            };
            
            for (var y = 0; y < textureSize; y++)
            {                
                for (var x = 0; x < textureSize; x++)
                {
                    var xSamp = random + (float)x / textureSize * noiseScale;
                    var ySamp = random + (float)y / textureSize * noiseScale;
    
                    var color = Mathf.PerlinNoise(xSamp, ySamp);
    
                    if (color <= rounding)
                        color = 0;
                    
                    texture.SetPixel(x,y,new Color(color, color, color,color));
                }
            }
    
            texture.Apply();
    
            return texture;
        }
        public Texture2D DrawOctavePerlin(float noiseScale,float noise = 0.4f,TextureFormat format = TextureFormat.RGBA32)
        {
            var random = Random.Range(-1000f, 1000f);
            
            var texture = new Texture2D(textureSize, textureSize, format, false)
            {
                filterMode = FilterMode.Point,
                name = "ProceduralPerlin"
            };
            
            for (var y = 0; y < textureSize; y++)
            {                
                for (var x = 0; x < textureSize; x++)
                {
                    var xSamp = random + (float)x / textureSize * noiseScale;
                    var ySamp = random + (float)y / textureSize * noiseScale;

                    var color = OctavePerlinNoise(xSamp, ySamp, noise);
    
                    texture.SetPixel(x,y,new Color(color, color, color,color));
                }
            }
    
            texture.Apply();
    
            return texture;
        }
    
        #region Noise
    
        public float OctavePerlinNoise(float x, float y,float noise = 0.4f)
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
        
        
        public Texture2D MergeTexture(Texture2D background, Texture2D overlay)
        {
            var startX = 0;
            var startY = background.height - overlay.height;
    
            for (var x = startX; x < background.width; x++)
            {
                for (var y = startY; y < background.height; y++)
                {
                    var bgColor = background.GetPixel(x, y);
                    var olColor = overlay.GetPixel(x, y);
    
                    var finalColor = Color.Lerp(bgColor, olColor, olColor.a / 1.0f);
    
                    background.SetPixel(x, y, finalColor);
                }
            }
    
            background.Apply();
            return background;
        }
    
        public Texture2D Draw(TextureFormat format = TextureFormat.RGB24)
        {
            var texture = new Texture2D(textureSize, textureSize, format, false)
            {
                filterMode = FilterMode.Bilinear,
                name = "ProceduralHeightMap"
            };
    
            foreach (var key in map.Keys)
            {        
                for (var y = 0; y < size; y++)
                {                
                    for (var x = 0; x < size; x++)
                    {
                        var val = Get(key, x, y);                                        
                        texture.SetPixel(x, y, Brightness(val));
                    }
                }
            }
            
            texture.Apply();
    
            return texture;
        }
        
        #region Draw
    
        private Color Brightness(float value)
        {
            var val = Mathf.Floor(value / textureSize);
            val = Mathf.Clamp01(val);
            val = Mathf.Lerp(val, 1, value / textureSize);
            
            return new Color(val, val, val, val);
        }
    
        #endregion
        
        public void GenerateTerrain(Texture2D heightMap,GameObject plane,GameObject sea,float height = 100,int size = 250)
        {
            var verts = new List<Vector3>();
            InitializeTerrain(heightMap, plane, height, size, ref verts);
            
            var average = verts.Select(vert => vert.y).Average();

            var localPosition = sea.transform.localPosition;
            localPosition = plane.GetComponent<Renderer>().bounds.center;
            localPosition = new Vector3(localPosition.x, average, localPosition.z);
            sea.transform.localPosition = localPosition;
        }
        
        public void GenerateCave(Texture2D heightMap,GameObject plane,float height = 100,int size = 250)
        {
            var verts = new List<Vector3>();
            InitializeTerrain(heightMap, plane, height, size,ref verts);
        }
    
    
        #region GenerateTerrain
    
        private void InitializeTerrain(Texture2D heightMap,GameObject plane,float height,int size,ref List<Vector3> verts)
        {
            var tris = new List<int>();

            size /= 2;
            
            for(var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                verts.Add(new Vector3(i, heightMap.GetPixel(i, j).grayscale * height / 3, j));

                if (i == 0 || j == 0)
                    continue;

                tris.Add(size * i + j);
                tris.Add(size * i + j - 1);
                tris.Add(size * (i - 1) + j - 1);
                tris.Add(size * (i - 1) + j - 1);
                tris.Add(size * (i - 1) + j);
                tris.Add(size * i + j);
            }

            var uvs = new Vector2[verts.Count];
            
            for (var i = 0; i < uvs.Length; i++)
                uvs[i] = new Vector2(verts[i].x, verts[i].z);
    
            if (!plane.GetComponent<MeshFilter>())
                plane.AddComponent<MeshFilter>();
    
            if (!plane.GetComponent<MeshRenderer>())
                plane.AddComponent<MeshRenderer>();

            if (!plane.GetComponent<MeshCollider>())
                plane.AddComponent<MeshCollider>();
            
            var procMesh = new Mesh
            {
                vertices = verts.ToArray(),
                uv = uvs,
                triangles = tris.ToArray()
            };
            
            procMesh.RecalculateNormals();
            plane.GetComponent<MeshFilter>().mesh = procMesh;
            plane.GetComponent<MeshCollider>().sharedMesh = procMesh;
        }
    
        #endregion
        
        public Texture2D Resize(Texture2D texture, int width, int height)
        {
            var rt = RenderTexture.GetTemporary(width, height);
            
            var preRT = RenderTexture.active;
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            var ret = new Texture2D(width, height);
            ret.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            ret.Apply();
            RenderTexture.active = preRT;

            RenderTexture.ReleaseTemporary(rt);
            return ret;
        }
    }
}