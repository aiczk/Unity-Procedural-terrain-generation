using System;
using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class LmHeightMap : ILandMapEffector
    {
        public Texture2D HeightMap;
        private readonly TextureFormat format;

        public LmHeightMap(TextureFormat format = TextureFormat.RGB24)
        {
            this.format = format;
        }
        
        void ILandMapEffector.Effect(LandMap landMap)
        {
            var texture = LandMapUtility.CreateTexture(LandMap.TextureSize, "ProceduralHeightMap", FilterMode.Bilinear, format);
    
            for (var y = 0; y < LandMap.Size; y++)
            for (var x = 0; x < LandMap.Size; x++)
                texture.SetPixel(x, y, Brightness(LandMap.TextureSize, landMap.GetHeight(x, y)));

            texture.Apply();
            HeightMap = texture;
        }
        
        private Color Brightness(int textureSize, float value)
        {
            var val = Mathf.Floor(value / textureSize);
            val = Mathf.Clamp01(val);
            val = Mathf.Lerp(val, 1, value / textureSize);
            return new Color(val, val, val, val);
        }
    }
}