using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Auto.LM;

public class TEST : MonoBehaviour
{
    [SerializeField] private Image image = default;
    [SerializeField] private GameObject ground = default,sea = default;
    [SerializeField, Range(0.2f, 2)] private float altitude = default;
    [SerializeField, Range(1, 5)] private int smooth = default;
    [Space(20)] 
    [SerializeField] private GameObject cave = default;
    [SerializeField] private Image perlin = default;
    [SerializeField, Range(10, 50)] private int perlinScale = default;
    [SerializeField, Range(40, 80)] private int caveHeight = default;
    [SerializeField, Range(0.2f, 2)]private float caveAltitude = default;
    [SerializeField, Range(1, 5)] private int caveSmooth = default;
    
    private LandMap landMap = new LandMap(500);
    private LandMap caveMap = new LandMap(500);

    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        landMap.Generate("m1",altitude);
        landMap.Smooth(smooth, "m1", "m2");
        
        var tex2d = landMap.Draw();
        var sprite = Sprite.Create(tex2d, new Rect(0, 0, 255, 255), Vector2.zero);
        landMap.GenerateTerrain(tex2d, ground, sea);
        image.sprite = sprite;

        
        caveMap.Generate("v1",caveAltitude);
        caveMap.Smooth(caveSmooth,"v1","v2");
        
        var caveTex = caveMap.Draw(TextureFormat.RGBA32);
        var octavePerlinTex = caveMap.DrawOctavePerlin(perlinScale);
        var perlinTex = caveMap.DrawPerlin(perlinScale);
        var merge = caveMap.MergeTexture(perlinTex,caveTex);
        merge = caveMap.MergeTexture(octavePerlinTex, merge);
        
        var spr = Sprite.Create(merge, new Rect(0, 0, 255, 255), Vector2.zero);
        caveMap.GenerateCave(merge, cave, caveHeight);
        perlin.sprite = spr;
    }
}
