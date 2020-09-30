# Unity Procedural terrain generation

An asset that allows you to easily generate terrain automatically.

  - Corresponds to compositing height maps.
  - You can combine Perlin and Octave Perlin noise to fabricate the generated terrain.

### Intoroduction

Basic Generation
```cs
LandMap landMap = new LandMap();

landMap.Initialize(altitude);
landMap.Smoothness(smooth);

var tex2d = landMap.HeightMap();
var mesh = landMap.CreateMesh(tex2d, height, size);

gameObject.GetComponent<MeshFilter>().mesh = mesh;
gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
```

Add Perlin
```cs
LandMap landMap = new LandMap();

landMap.Initialize(altitude);
landMap.Smoothness(smooth);

var tex2d = landMap.HeightMap();
var perlin = landMap.Perlin(noise);
tex2d = landMap.MergeTexture(tex2d, perlin);

var mesh = landMap.CreateMesh(tex2d, height, size);

gameObject.GetComponent<MeshFilter>().mesh = mesh;
gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
```



## Feature Outlook and Todo

- Ensureing the scalability and maintainability of each interface.
- Refactoring.
- Support for DOTS!

License
----

MIT