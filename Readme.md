# Unity Procedural terrain generation

An asset that allows you to easily generate terrain automatically.

  - Corresponds to compositing height maps.
  - You can combine Perlin and Octave Perlin noise to fabricate the generated terrain.

### Intoroduction

Basic Generation
```cs
var lmHeightMap = new LMHeightMap();

var mesh = landMap
            .Initialize(altitude)
            .AddEffect(new Smoothness(smooth))
            .AddEffect(new PerlinNoise(perlin))
            .AddEffect(new OctavePerlinNoise(octave))
            .AddEffect(lmHeightMap)
            .CreateMesh(lmHeightMap, height, size);

image.sprite = Sprite.Create(lmHeightMap.HeightMap, new Rect(0, 0, 255, 255), Vector2.zero);
            
gameObject.GetComponent<MeshFilter>().mesh = mesh;
gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
```

The current version includes the three features listed above, and an easily extendable interface if it is missing.

### Extension

```c#
public interface ILandMapEffect
{
  void Effect(Landmap landMap);
}
```

Inheriting the above interface, you can use the Get/SetHeight methods in LandMap to make changes to Landmap. The extensions you create can be easily used with LandMap. 

For an extended example, see [PerlinNoise.cs](https://github.com/aiczk/Unity-Procedural-terrain-generation/tree/master/Procedural/Assets/ProceduralGeneration/Effect/PerlinNoise.cs) in ProceduralGeneration.Effect.



## Feature Outlook and Todo

- Support for DOTS!



License
----

MIT