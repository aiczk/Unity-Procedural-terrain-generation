using System.Diagnostics;
using ProceduralGeneration;
using ProceduralGeneration.Effect;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Scene
{
    public class GenerateTest : MonoBehaviour
    {
        [SerializeField] private Image image = default;
        [SerializeField, Range(0.2f, 2)] private float altitude = default;
        [SerializeField, Range(10, 50)] private float perlin = 10, octave = 10;
        [SerializeField, Range(1, 5)] private int smooth = 1;
        [SerializeField, Range(50, 250)] private int height = 50, size = 50;
    
        private readonly LandMap landMap = new LandMap();

        private void Awake()
        {
            var lmHeightMap = new LmHeightMap();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var mesh = landMap
                .Initialize(altitude)
                //.AddEffect(new Smooth(smooth))
                //.AddEffect(new PerlinNoise(perlin))
                //.AddEffect(new OctavePerlinNoise(octave))
                .AddEffect(new ComplexErosion(1.5f, 0.03f, 3, 8000000))
                .AddEffect(lmHeightMap)
                .CreateMesh(new LmMesh(lmHeightMap, size, height));
            
            stopWatch.Stop();
            Debug.Log($"{stopWatch.ElapsedMilliseconds.ToString()}ms");
            
            image.sprite = Sprite.Create(lmHeightMap.HeightMap, new Rect(0, 0, 256, 256), Vector2.zero);
            
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
