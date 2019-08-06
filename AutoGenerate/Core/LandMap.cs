using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Auto.Dijkstra;
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
    
        private int IndexOfMin(float[] arr)
        {
            if (arr.Length == 0)
                return -1;
    
            var min = arr[0];
            var minIndex = 0;
    
            for (var ind = 1; ind < arr.Length; ind++)
            {
                if (!(arr[ind] < min))
                    continue;
                
                minIndex = ind;
                min = arr[ind];
            }
    
            return minIndex;
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
    
        public void Combine(string one,string two,string three,string result)
        {
            if (!map.ContainsKey(result))
            {
                var arraySize = size * size;
                map.Add(result,new float[(int)arraySize]);
            }
    
            var max = float.MaxValue;
            var min = float.MinValue;
    
            for (var y= 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var val = Get(two, x, y);
    
                    max = Mathf.Max(max, val);
                    min = Mathf.Min(min, val);
                }
            }
            
            for (var y= 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var featureOne = Get(one, x, y);
                    var featureTwo = Get(two, x, y);
                    var featureThree = Get(three, x, y);
    
                    var percent = Percent(featureThree, min, max);
                    var val = (1 - percent) * featureOne + percent * featureTwo;
                    Set(result, x, y, val);
                }
            }
        }
    
        #region Combine
    
        private float Percent(float value,float min,float max)
        {
            return value / Mathf.Abs((max - min));
        }
        
        #endregion
    
        public void ComplexErosion(float carryingCapacity,float depositionSpeed,int iterations,int drops,string from,string to)
        {
            var heightMap = new float[(int) (size * size)];
            
            for (var y= 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var index = (x + size * y);
                    heightMap[(int) index] = Get(from, x, y);
                }
            }
    
            for (var drop = 0; drop < drops; drop++)
            {
                heightMap = Deposit_At((int) Mathf.Floor(RandomValue() * size), (int) Mathf.Floor(RandomValue() * size),
                                       iterations, heightMap, depositionSpeed, carryingCapacity);
                map[to] = heightMap;
            }
        }
    
        #region complexErosion
    
        private int HMapIndex(int x, int y)
        {
            var index = (x + size * y);
            return (int) index;
        }
    
        private float HMapValue(float[] HMap,int x,int y)
        {
            var index = (x + size * y);
            return HMap[(int) index];
        }
    
        private float[] Deposit_At(int x, int y,int iterations,float[] HMap,float kd,float kq)
        {
            var c = 0.0f;
            var v = 1.05f;
            var minSlope = 1.15f;
            var maxVelocity = 10.0f;
    
            for (var iter = 0; iter < iterations; iter++)
            {
                v = Mathf.Min(v, maxVelocity);
                var val = HMapValue(HMap, x, y);
    
                float[] nv = 
                {
                    HMapValue(HMap, x, y - 1),
                    HMapValue(HMap, x, y + 1),
                    HMapValue(HMap, x + 1, y),
                    HMapValue(HMap, x - 1, y)
                };
    
    //            DEBUG
    //            float s;
    //
    //            s = HMapValue(HMap, x, y - 1);
    //            s = HMapValue(HMap, x, y + 1);
    //            s = HMapValue(HMap, x + 1, y);
    //            s = HMapValue(HMap, x - 1, y);
                
    
                var minInd = IndexOfMin(nv);
    
                if (!(nv[minInd] < val)) 
                    continue;
    
                var slope = Mathf.Min(minSlope, val - nv[minInd]);
                var vtc = kd * v * slope;
    
                if (c > kq)
                {
                    c -= vtc;
                    HMap[HMapIndex(x, y)] += vtc;
                }
                else
                {
                    if (c + vtc > kq)
                    {
                        var delta = c + vtc - kq;
                        c += delta;
                        HMap[HMapIndex(x, y)] -= delta;
                    }
                    else
                    {
                        c += vtc;
                        HMap[HMapIndex(x, y)] -= vtc;
                    }
                }
    
                switch (minInd)
                {
                    case 0:
                        y -= 1;
                        break;
                    case 1:
                        y += 1;
                        break;
                    case 2:
                        x += 1;
                        break;
                    case 3:
                        x -= 1;
                        break;
                }
    
                if (x > size - 1) 
                    x = (int) size;
                
                if (y > size - 1) 
                    y = (int) size;
    
                if (x < 0) 
                    x = 0;
    
                if (y < 0) 
                    y = 0;
            }
    
            return HMap;
        }
    
        #endregion
        
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
            var max = 0.0f;
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
                    var olColor = overlay.GetPixel(x - startX, y - startY);
    
                    var final_color = Color.Lerp(bgColor, olColor, olColor.a / 1.0f);
    
                    background.SetPixel(x, y, final_color);
                }
            }
    
            background.Apply();
            return background;
        }
    
        public Texture2D Draw(TextureFormat format = TextureFormat.RGB24)
        {
            var texture = new Texture2D(textureSize, textureSize, format, false)
            {
                filterMode = FilterMode.Point,
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
        
        public void GenerateTerrain(Texture2D heightMap,GameObject plane,GameObject sea,float height = 100,int extraction = 100,int size = 250)
        {
            var verts = new List<Vector3>();
            InitializeTerrain(heightMap, plane, height, size, ref verts);
            
            var average = verts.Select(vert => vert.y).Average();
            
            sea.transform.position = plane.GetComponent<Renderer>().bounds.center;
            sea.transform.position = new Vector3(sea.transform.position.x, average, sea.transform.position.z);
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
     
            for(var i = 0; i < size; i++)
            {
                for(var j = 0; j < size; j++)
                {
                    verts.Add(new Vector3(i, heightMap.GetPixel(i,j).grayscale * height, j));
                    if (i == 0 || j == 0) 
                        continue;
                    
                    tris.Add(size * i + j);
                    tris.Add(size * i + j - 1);
                    tris.Add(size * (i - 1) + j - 1);
                    tris.Add(size * (i - 1) + j - 1);
                    tris.Add(size * (i - 1) + j);
                    tris.Add(size * i + j);
                }
            }
     
            var uvs = new Vector2[verts.Count];
            
            for (var i = 0; i < uvs.Length; i++)
                uvs[i] = new Vector2(verts[i].x, verts[i].z);
    
            if (!plane.GetComponent<MeshFilter>())
                plane.AddComponent<MeshFilter>();
    
            if (!plane.GetComponent<MeshRenderer>())
                plane.AddComponent<MeshRenderer>();
    
            var procMesh = new Mesh
            {
                vertices = verts.ToArray(),
                uv = uvs,
                triangles = tris.ToArray()
            };
            
            procMesh.RecalculateNormals();
            plane.GetComponent<MeshFilter>().mesh = procMesh;
        }
    
        #endregion
    }
}