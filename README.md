# chunk-generator
## Overview

A set of ScriptableObjects and a generator that produces voxel chunks using Perlin noise, with support for layers, ores, caves, and arbitrary structures.

## Quick Start

1. In Unity, create a **Chunk Config** via **Create → Chunks → Chunk Config**.
2. Set **Width** and **Length**, define your **layers** and **structures**, and assign the **Random Perlin Generator** to **Generator**.
3. Create **Block Config** assets via **Create → Chunks → Block Config** and assign materials, strength, and transparency.
4. Optionally, configure **Ore Config** within each layer (chance, totals, cluster size).
5. Create **Structure Config** assets via **Create → Chunks → Structure Config**, set size, anchor, matrix and templates.
6. Tweak generator parameters (scale, seed, heights, caves, spawn rates) on the **Random Perlin Generator** asset.
7. Call `generator.Generate(yourChunkConfig, out maxHeight)` from your MonoBehaviour to fill a `Block[,,]` array.

## Script Reference

### BlockConfig.cs
```csharp
using UnityEngine;
[CreateAssetMenu(menuName="Chunks/Block Config")]
public class BlockConfig : ScriptableObject
{
    public Material material;
    public int strength;
    public bool IsTransparent = false;
}
```

### StructureConfig.cs
```csharp
using UnityEngine;
[CreateAssetMenu(menuName="Chunks/Structure Config")]
public class StructureConfig : ScriptableObject
{
    public SerializableVector3Int sizeRaw;
    public SerializableVector3Int anchorRaw;
    public int[] serializedMatrix;
    public BlockConfig[] blockTemplates;
    public bool flattenGroundUnderStructure = true;

    public Vector3Int size => sizeRaw.ToVector3Int();
    public Vector3Int anchor => anchorRaw.ToVector3Int();

    public int GetIndex(int x,int y,int z) => x + size.x * (y + size.y * z);
    public int GetValue(int x,int y,int z) => serializedMatrix[GetIndex(x,y,z)];
    public void SetValue(int x,int y,int z,int value) => serializedMatrix[GetIndex(x,y,z)] = value;
}
```

### ChunkConfig.cs
```csharp
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Chunks/Chunk Config")]
public class ChunkConfig : ScriptableObject
{
    public int Width = 16;
    public int Length = 16;
    public List<ChunkLayer> layers;
    public List<ChunkStructure> structures;
    public List<BlockConfig> globalBlockConfigs;
    public ChunkGenerator generator;
}
```

### RandomPerlinChunkGenerator.cs
```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Text;
[CreateAssetMenu(menuName="Chunks/Random Perlin Generator")]
public class RandomPerlinChunkGenerator : ChunkGenerator
{
    public float scale = 16f;
    public bool randomizeSeed = true;
    public int seed = 12345;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 offset;
    public int maxTerrainHeight = 32;
    public int chunkMaxHeight = 64;
    public int zeroHeight = 16;
    public int maxDown = 8;
    public bool enableCaves = false;
    public float caveScale = 16f;
    public int caveOctaves = 3;
    public float cavePersistence = 0.5f;
    public float caveLacunarity = 2f;
    public Vector3 caveOffset;
    public float caveThreshold = 0.6f;
    public int caveMinDepth = 2;

    public override Block[,,] Generate(ChunkConfig config,out int maxHeight)
    {
        InitGlobals(config,out var globalBlockConfigs,out var map,out var prng);
        GenerateHeightNoise(config,prng,out var heightMap);
        var blocks = GenerateBaseTerrain(config,heightMap,map,globalBlockConfigs,prng,out var oreVeins,out var oreCounts);
        if(enableCaves) GenerateCaves(blocks,prng);
        var structureCoords = PlaceStructures(config,blocks,heightMap,map,globalBlockConfigs,prng);
        LogReport(structureCoords,oreVeins,oreCounts);
        config.globalBlockConfigs = globalBlockConfigs;
        maxHeight = chunkMaxHeight;
        return blocks;
    }
}
```

### OreConfig.cs
```csharp
using UnityEngine;
[System.Serializable]
public class OreConfig
{
    public BlockConfig block;
    [Range(0f,1f)] public float chance;
    public int minTotalOres;
    public int maxTotalOres;
    public int clusterSize;
    public float noiseScale;
}
```

### ChunkLayer.cs
```csharp
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ChunkLayer
{
    public BlockConfig bloc;
    public int thickness;
    public List<OreConfig> ores;
}
```

## License

Distributed under the MIT License. See [LICENSE](LICENSE).

