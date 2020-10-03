# Unity Procedural terrain generation

An asset that allows you to easily generate terrain automatically.

  - Corresponds to compositing height maps.
  - You can combine Perlin and Octave Perlin noise to fabricate the generated terrain.

### Intoroduction

Basic Generation
```cs
var landMap = new LandMap();
var lmHeightMap = new LmHeightMap();

var mesh = landMap
            .Initialize(altitude)
            .AddEffect(new Smooth(smoothLevel))
            .AddEffect(new PerlinNoise(perlin))
            .AddEffect(new OctavePerlinNoise(octave))
            .AddEffect(new Combine(otherLandMap))
            .AddEffect(new ComplexErosion(carryingCapacity, depositionSpeed, iterationCount, drop))
            .AddEffect(lmHeightMap)
            .CreateMesh(new LmMesh(lmHeightMap, size, height));

image.sprite = Sprite.Create(lmHeightMap.HeightMap, new Rect(0, 0, 255, 255), Vector2.zero);
            
gameObject.GetComponent<MeshFilter>().mesh = mesh;
gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
```

The current version includes the five features listed above, and an easily extendable interface if it is missing.

### Extension

```c#
public interface ILandMapEffector
{
  void Effect(Landmap landMap);
}
```

Inheriting the above interface, you can use the Get/SetHeight methods in LandMap to make changes to Landmap. The extensions you create can be easily used with LandMap. 

For an extended example, see [PerlinNoise.cs](https://github.com/aiczk/Unity-Procedural-terrain-generation/tree/master/Procedural/Assets/ProceduralGeneration/Effect/PerlinNoise.cs) in ProceduralGeneration.Effect.



## Feature Outlook and Todo

- Support for DOTS!
- Improved generation method when not using HeightMap.



License
----

MIT