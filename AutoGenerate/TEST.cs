using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Auto.LM;
using Sirenix.OdinInspector;

public class TEST : MonoBehaviour
{
    [SerializeField] private Image image = default;
    [SerializeField] private GameObject ground = default,sea = default;
    [SerializeField, Range(0.2f, 2)] private float altitude = default;
    [SerializeField, Range(1, 5)] private int smooth = default;
    [SerializeField, Range(50, 250)] private int size = default,height = default;
    
    private LandMap landMap = new LandMap(500);
    
    [Button]
    private void Generate()
    {
        landMap.Generate("m1",altitude);
        landMap.Smooth(smooth, "m1", "m2");
        
        var tex2d = landMap.Draw();
        var sprite = Sprite.Create(tex2d, new Rect(0, 0, 255, 255), Vector2.zero);
        landMap.GenerateTerrain(tex2d, ground, sea, height, size);
        image.sprite = sprite;
    }
}
