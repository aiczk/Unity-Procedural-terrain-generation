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
        
        [Header("LandMap Settings")]
        [SerializeField, Range(0.2f, 2)] private float altitude = default;
        [SerializeField, Range(50, 250)] private int height = 50, size = 50;
        
        [Space, Header("Octave/PerlinNoise Settings")]
        [SerializeField, Range(10, 50)] private float perlin = 10, octave = 10;
        
        [Space, Header("Smooth Level")]
        [SerializeField, Range(1, 5)] private int smooth = 1;

        [Space, Header("ComplexErosion Settings")] 
        [SerializeField, Range(1, 3)] private float carryingCapacity = 1;
        [SerializeField, Range(0, 0.1f)] private float depositionSpeed = 0;
        [SerializeField, Range(1, 3)] private int iterationCount = 1;
        [SerializeField, Range(0, 8000000)] private int drop = 0;
    
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
                .AddEffect(new ComplexErosion(carryingCapacity, depositionSpeed, iterationCount, drop))
                .AddEffect(lmHeightMap)
                .CreateMesh(new LmMesh(lmHeightMap, size, height));
            
            stopWatch.Stop();
            Debug.Log($"process: {stopWatch.ElapsedMilliseconds.ToString()}ms");
            
            image.sprite = Sprite.Create(lmHeightMap.HeightMap, new Rect(0, 0, 256, 256), Vector2.zero);
            
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
