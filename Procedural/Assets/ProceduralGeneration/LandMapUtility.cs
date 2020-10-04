using UnityEngine;

namespace ProceduralGeneration
{
    public static class LandMapUtility
    {
        public static Texture2D Resize(Texture2D texture, int size)
        {
            var temp = RenderTexture.GetTemporary(size, size);
            var preRt = RenderTexture.active;
            
            RenderTexture.active = temp;
            
            Graphics.Blit(texture, temp);
            var tex = new Texture2D(size, size);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            
            RenderTexture.active = preRt;
            RenderTexture.ReleaseTemporary(temp);
            
            return tex;
        }

        public static Texture2D MergeTexture(Texture2D background, Texture2D overlay)
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

        public static Texture2D CreateTexture(int textureSize, string name, FilterMode filterMode = FilterMode.Point, TextureFormat format = TextureFormat.RGBA32) =>
            new Texture2D(textureSize, textureSize, format, false)
            {
                filterMode = filterMode,
                name = name
            };
    }
}