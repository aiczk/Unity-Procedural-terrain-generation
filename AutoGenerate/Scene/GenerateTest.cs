using Procedural;
using UnityEngine;
using UnityEngine.UI;

namespace AutoGenerate
{
    public class GenerateTest : MonoBehaviour
    {
        [SerializeField] private Image image = default;
        [SerializeField, Range(0.2f, 2)] private float altitude = default;
        [SerializeField, Range(1, 5)] private int smooth = default;
        [SerializeField, Range(50, 250)] private int size = default,height = default;
    
        private LandMap landMap = new LandMap();
    
        private void Awake()
        {
            landMap.SetUp(altitude);
            landMap.Smoothness(smooth);
        
            var tex2d = landMap.HeightMap();
            var sprite = Sprite.Create(tex2d, new Rect(0, 0, 255, 255), Vector2.zero);
            image.sprite = sprite;
            
            var mesh = landMap.CreateMesh(tex2d, height, size);
            
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}
