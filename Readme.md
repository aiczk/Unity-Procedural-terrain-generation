# Unity Procedural terrain generation

An asset that allows you to easily generate terrain automatically.

  - Corresponds to compositing height maps.
  - You can combine Perlin and Octave Perlin noise to fabricate the generated terrain.

### Intoroduction

Terrain Generate
```cs
    private LandMap landMap = new LandMap(MapSize);
    private GameObject ground,sea;
    
    landMap.Generate("Base",altitude);
    landMap.Smooth(smooth, "Base", "New");
    
    Texture2D tex2d = landMap.Draw();
    landMap.GenerateTerrain(tex2d, ground, sea);
```

Cave Generate
```cs
    private LandMap caveMap = new LandMap(MapSize);
    private GameObject cave;
    
    caveMap.Generate("v1",caveAltitude);
    caveMap.Smooth(caveSmooth,"v1","v2");
    
    var caveTex = caveMap.Draw(TextureFormat.RGBA32);
    var octavePerlinTex = caveMap.DrawOctavePerlin(perlinScale);
    var perlinTex = caveMap.DrawPerlin(perlinScale);
    var merge = caveMap.MergeTexture(perlinTex,caveTex);
    merge = caveMap.MergeTexture(octavePerlinTex, merge);
    
    caveMap.GenerateCave(merge, cave, caveHeight);
```

License
----

MIT
