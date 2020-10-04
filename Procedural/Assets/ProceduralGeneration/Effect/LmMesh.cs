using UnityEngine;

namespace ProceduralGeneration.Effect
{
    public class LmMesh : ILandMapEffector
    {
        public Mesh LandMapMesh { get; private set; }
        private readonly LmHeightMap lmHeightMap;
        private readonly int size;
        private readonly float height;

        public LmMesh(LmHeightMap lmHeightMap, int size = 100, float height = 100)
        {
            this.lmHeightMap = lmHeightMap;
            this.size = size;
            this.height = height;
        }

        public void Effect(LandMap landMap)
        {
            var heightMap = lmHeightMap.HeightMap;
            
            if (LandMap.TextureSize != size) 
                heightMap = LandMapUtility.Resize(heightMap, size);

            var halfSize = Mathf.CeilToInt((float) size / 2);
            
            var verticesSize = halfSize * halfSize;
            
            var triangles = new int[(verticesSize - (size - 1)) * 6];
            var vertices = new Vector3[verticesSize];
            var uv = new Vector2[verticesSize];
            
            var vert = 0;
            var tri = 0;
            
            for (var i = 0; i < halfSize; i++)
            for (var j = 0; j < halfSize; j++, ++vert)
            { 
                //vertices[vert] = new Vector3(i, (range - GetHeight(j, i)) / range * height / 2, j);
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
            
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                name = "LandMap"
            };
            
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.Optimize();

            LandMapMesh = mesh;

            void Increment(int value)
            {
                triangles[tri] = value;
                tri++;
            }
        }
        
    }
}