using System.Diagnostics;
using ProceduralGeneration;
using ProceduralGeneration.Effect;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Scene
{
    public class GenerateTest : MonoBehaviour
    {
        [Header("HeightMap Settings")]
        [SerializeField] private Image heightMap = default;
        
        [Space, Header("LandMap Settings")]
        [SerializeField, Range(0.2f, 2)] private float deviation = 0.2f;
        [SerializeField, Range(100, 250)] private int size = 100;
        [SerializeField, Range(50, 150)] private int height = 50;

        [Space, Header("Octave/PerlinNoise Settings")] 
        [SerializeField, Range(10, 50)] private float perlin = 10f;
        [SerializeField, Range(10, 50)] private float octave = 10f;
        
        [Space, Header("Fractal Brownian Motion(fBM) Settings")] 
        [SerializeField, Range(1, 10)] private int octaves = 1;
        [SerializeField, Range(2, 7)] private float lacunarity = 2;
        [SerializeField, Range(-1, 0.5f)] private float gain = -1;
        
        [Space, Header("ComplexErosion Settings")] 
        [SerializeField, Range(1, 3.5f)] private float carryingCapacity = 1;
        [SerializeField, Range(0, 0.3f)] private float depositionSpeed = 0;
        [SerializeField, Range(1, 3)] private int iterationCount = 1;
        [SerializeField, Range(0, 8000000)] private int drop = 0;

        [Space, Header("Voronoi Settings")] 
        [SerializeField, Range(0, 1)] private int u = 0;
        [SerializeField, Range(0, 1)] private int v = 0;
        
        [Space, Header("Smooth Setting")]
        [SerializeField, Range(1, 5)] private int smoothLevel = 1;

        private readonly LandMap landMap = new LandMap();
        private readonly LmHeightMap lmHeightMap = new LmHeightMap();

        private void Awake()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var mesh = landMap
                .Initialize(deviation)
                .AddEffect(new FractalBrownianMotion(octaves, lacunarity, gain))
                .AddEffect(new PerlinNoise(perlin))
                .AddEffect(new OctavePerlinNoise(octave))
                .AddEffect(new ComplexErosion(carryingCapacity, depositionSpeed, iterationCount, drop))
                .AddEffect(new Smooth(smoothLevel))
                .AddEffect(lmHeightMap)
                .CreateMesh(new LmMesh(lmHeightMap, size, height));
            
            stopWatch.Stop();
            Debug.Log($"process time: {stopWatch.ElapsedMilliseconds.ToString()}ms");
            
            heightMap.sprite = Sprite.Create(lmHeightMap.HeightMap, new Rect(0, 0, 257, 257), Vector2.zero);

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
