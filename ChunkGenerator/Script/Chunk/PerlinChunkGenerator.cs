using UnityEngine;
using System.Collections.Generic;
using System.Text;

[CreateAssetMenu(menuName = "Chunks/Random Perlin Generator")]
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

    public override Block[,,] Generate(ChunkConfig config, out int maxHeight)
    {
        InitGlobals(config, out var globalBlockConfigs, out var map, out var prng);
        GenerateHeightNoise(config, prng, out var heightMap);
        var blocks = GenerateBaseTerrain(config, heightMap, map, globalBlockConfigs, prng, out var oreVeins, out var oreCounts);
        if (enableCaves) GenerateCaves(blocks, prng);
        var structureCoords = PlaceStructures(config, blocks, heightMap, map, globalBlockConfigs, prng);
        LogReport(structureCoords, oreVeins, oreCounts);
        config.globalBlockConfigs = globalBlockConfigs;
        maxHeight = chunkMaxHeight;
        return blocks;
    }

    void InitGlobals(ChunkConfig config, out List<BlockConfig> globalBlockConfigs, out Dictionary<BlockConfig,int> map, out System.Random prng)
    {
        prng = new System.Random(randomizeSeed ? Random.Range(int.MinValue, int.MaxValue) : seed);
        globalBlockConfigs = new List<BlockConfig>();
        map = new Dictionary<BlockConfig,int>();
        foreach (var layer in config.layers)
            if (!map.ContainsKey(layer.bloc))
            {
                map[layer.bloc] = globalBlockConfigs.Count;
                globalBlockConfigs.Add(layer.bloc);
            }
        foreach (var st in config.structures)
            if (st.config != null)
                foreach (var bc in st.config.blockTemplates)
                    if (!map.ContainsKey(bc))
                    {
                        map[bc] = globalBlockConfigs.Count;
                        globalBlockConfigs.Add(bc);
                    }
        foreach (var layer in config.layers)
            if (layer.ores != null)
                foreach (var ore in layer.ores)
                    if (ore.block != null && !map.ContainsKey(ore.block))
                    {
                        map[ore.block] = globalBlockConfigs.Count;
                        globalBlockConfigs.Add(ore.block);
                    }
    }

    void GenerateHeightNoise(ChunkConfig config, System.Random prng, out int[,] heightMap)
    {
        int w = config.Width, l = config.Length;
        float[,] noise = new float[w,l];
        heightMap = new int[w,l];
        Vector2[] offs2d = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
            offs2d[i] = new Vector2(prng.Next(-100000,100000) + offset.x, prng.Next(-100000,100000) + offset.y);
        float minN = float.MaxValue, maxN = float.MinValue;
        for (int x = 0; x < w; x++)
            for (int z = 0; z < l; z++)
            {
                float n = 0, amp = 1, freq = 1, sum = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sx = (x + offs2d[i].x) / scale * freq;
                    float sz = (z + offs2d[i].y) / scale * freq;
                    n += (Mathf.PerlinNoise(sx,sz) * 2 - 1) * amp;
                    sum += amp; amp *= persistence; freq *= lacunarity;
                }
                n /= sum;
                noise[x,z] = n;
                minN = Mathf.Min(minN, n);
                maxN = Mathf.Max(maxN, n);
            }
        if (maxN - minN < 0.5f)
        {
            float mid = (maxN + minN) * 0.5f;
            minN = mid - 0.25f;
            maxN = mid + 0.25f;
        }
        for (int x = 0; x < w; x++)
            for (int z = 0; z < l; z++)
            {
                float t = Mathf.InverseLerp(minN, maxN, noise[x,z]);
                heightMap[x,z] = Mathf.Clamp(zeroHeight - maxDown + Mathf.RoundToInt(t * (maxTerrainHeight - (zeroHeight - maxDown))), zeroHeight - maxDown, maxTerrainHeight);
            }
    }

    Block[,,] GenerateBaseTerrain(ChunkConfig config, int[,] heightMap, Dictionary<BlockConfig,int> map, List<BlockConfig> globalBlockConfigs, System.Random prng, out Dictionary<BlockConfig,List<Vector3Int>> oreVeins, out Dictionary<BlockConfig,int> oreBlockCounts)
    {
        int w = config.Width, l = config.Length, h = chunkMaxHeight;
        var blocks = new Block[w, l, h];
        oreVeins = new Dictionary<BlockConfig,List<Vector3Int>>();
        oreBlockCounts = new Dictionary<BlockConfig,int>();
        foreach (var layer in config.layers)
            foreach (var ore in layer.ores)
                oreBlockCounts[ore.block] = 0;
        for (int x = 0; x < w; x++)
        for (int z = 0; z < l; z++)
        {
            int y = heightMap[x,z] - 1;
            for (int i = 0; i < config.layers.Count && y >= 0; i++)
            {
                var layer = config.layers[i];
                int cnt = Mathf.Min(layer.thickness, y + 1);
                int gi = map[layer.bloc];
                for (int j = 0; j < cnt; j++)
                {
                    blocks[x,z,y] = new Block(gi, layer.bloc);
                    if (layer.ores != null)
                        foreach (var ore in layer.ores)
                        {
                            int current = oreBlockCounts[ore.block];
                            if (current < ore.maxTotalOres && Random.value < ore.chance)
                            {
                                for (int c = 0; c < ore.clusterSize && oreBlockCounts[ore.block] < ore.maxTotalOres; c++)
                                {
                                    int dx = Mathf.Clamp(x + prng.Next(-1,2), 0, w-1);
                                    int dz = Mathf.Clamp(z + prng.Next(-1,2), 0, l-1);
                                    int dy = Mathf.Clamp(y + prng.Next(-1,2), heightMap[dx,dz] - layer.thickness, heightMap[dx,dz] - 1);
                                    if (blocks[dx,dz,dy] != null && blocks[dx,dz,dy].LayerIndex == gi)
                                    {
                                        int oreIdx;
                                        if (!map.TryGetValue(ore.block, out oreIdx))
                                        {
                                            oreIdx = globalBlockConfigs.Count;
                                            map[ore.block] = oreIdx;
                                            globalBlockConfigs.Add(ore.block);
                                        }
                                        blocks[dx,dz,dy] = new Block(oreIdx, ore.block, ore.block.IsTransparent);
                                        if (!oreVeins.ContainsKey(ore.block)) oreVeins[ore.block] = new List<Vector3Int>();
                                        oreVeins[ore.block].Add(new Vector3Int(dx,dy,dz));
                                        oreBlockCounts[ore.block]++;
                                    }
                                }
                            }
                        }
                    y--;
                }
            }
        }
        foreach (var layer in config.layers)
            foreach (var ore in layer.ores)
            {
                int current = oreBlockCounts[ore.block];
                int need = ore.minTotalOres - current;
                int gi = map[layer.bloc];
                for (int k = 0; k < need; k++)
                {
                    int attempts = 0;
                    while (attempts < 100)
                    {
                        attempts++;
                        int cx = prng.Next(0, w);
                        int cz = prng.Next(0, l);
                        int baseY = heightMap[cx,cz] - 1;
                        int bottom = baseY - layer.thickness + 1;
                        int cy = prng.Next(bottom, baseY + 1);
                        if (blocks[cx,cz,cy] != null && blocks[cx,cz,cy].LayerIndex == gi)
                        {
                            int oreIdx;
                            if (!map.TryGetValue(ore.block, out oreIdx))
                            {
                                oreIdx = globalBlockConfigs.Count;
                                map[ore.block] = oreIdx;
                                globalBlockConfigs.Add(ore.block);
                            }
                            blocks[cx,cz,cy] = new Block(oreIdx, ore.block);
                            if (!oreVeins.ContainsKey(ore.block)) oreVeins[ore.block] = new List<Vector3Int>();
                            oreVeins[ore.block].Add(new Vector3Int(cx,cy,cz));
                            oreBlockCounts[ore.block]++;
                            break;
                        }
                    }
                }
            }
        return blocks;
    }




    void GenerateCaves(Block[,,] blocks, System.Random prng)
    {
        int w = blocks.GetLength(0), l = blocks.GetLength(1), h = Mathf.Min(zeroHeight, blocks.GetLength(2));
        Vector3[] offs = new Vector3[caveOctaves];
        for (int i = 0; i < caveOctaves; i++)
            offs[i] = new Vector3(prng.Next(-100000,100000) + caveOffset.x, prng.Next(-100000,100000) + caveOffset.y, prng.Next(-100000,100000) + caveOffset.z);
        for (int x = 0; x < w; x++)
            for (int z = 0; z < l; z++)
                for (int y = caveMinDepth; y < h; y++)
                {
                    float n = 0, amp = 1, freq = 1, sum = 0;
                    for (int i = 0; i < caveOctaves; i++)
                    {
                        float nx = (x + offs[i].x) / caveScale * freq;
                        float ny = (y + offs[i].y) / caveScale * freq;
                        float nz = (z + offs[i].z) / caveScale * freq;
                        n += (Mathf.PerlinNoise(nx,ny) + Mathf.PerlinNoise(ny,nz) + Mathf.PerlinNoise(nz,nx)) / 3f * amp;
                        sum += amp; amp *= cavePersistence; freq *= caveLacunarity;
                    }
                    n /= sum;
                    if (n > caveThreshold && blocks[x,z,y] != null) blocks[x,z,y] = null;
                }
    }

    List<Vector3Int> PlaceStructures(ChunkConfig config, Block[,,] blocks, int[,] heightMap, Dictionary<BlockConfig,int> map, List<BlockConfig> globalBlockConfigs, System.Random prng)
    {
        int w = config.Width, l = config.Length, h = chunkMaxHeight;
        var structureCoords = new List<Vector3Int>();
        foreach (var st in config.structures)
            if (st.config != null)
            {
                int toPlace = prng.Next(st.minCount, st.maxCount + 1);
                var candidates = new List<Vector2Int>();
                for (int cx = 0; cx < w; cx++)
                    for (int cz = 0; cz < l; cz++)
                        candidates.Add(new Vector2Int(cx, cz));
                var placed = new List<Vector2Int>();
                int attempts = 0;
                var s = st.config.size; var anc = st.config.anchor;
                while (placed.Count < toPlace && attempts < candidates.Count)
                {
                    attempts++; int idx = prng.Next(candidates.Count);
                    var center = candidates[idx]; candidates.RemoveAt(idx);
                    if (Random.value > st.spawnChance) continue;
                    bool tooClose = false;
                    foreach (var p in placed)
                        if (Vector2Int.Distance(p, center) < st.minDistance) { tooClose = true; break; }
                    if (tooClose) continue;
                    int ox = center.x - anc.x, oz = center.y - anc.z;
                    if (ox < 0 || oz < 0 || ox + s.x > w || oz + s.z > l) continue;
                    int by = heightMap[center.x, center.y] - anc.y;
                    if (by < 0 || by + s.y > h) continue;
                    bool coll = false;
                    for (int x0 = 0; x0 < s.x && !coll; x0++)
                        for (int y0 = 0; y0 < s.y && !coll; y0++)
                            for (int z0 = 0; z0 < s.z; z0++)
                            {
                                int vIdx = x0 + s.x * (y0 + s.y * z0);
                                if (st.config.serializedMatrix[vIdx] > 0 && blocks[ox + x0, oz + z0, by + y0] != null)
                                    coll = true;
                            }
                    if (coll) continue;
                    for (int x0 = 0; x0 < s.x; x0++)
                        for (int y0 = 0; y0 < s.y; y0++)
                            for (int z0 = 0; z0 < s.z; z0++)
                            {
                                int vIdx = x0 + s.x * (y0 + s.y * z0);
                                int val = st.config.serializedMatrix[vIdx];
                                if (val == 0) continue;
                                int ti = val - 1;
                                if (ti < 0 || ti >= st.config.blockTemplates.Length) continue;
                                var bc = st.config.blockTemplates[ti];
                                blocks[ox + x0, oz + z0, by + y0] = new Block(map[bc], bc);
                            }
                    FillStructureFoundation(blocks, new Vector3Int(ox, by, oz), s, config, map, st.config);
                    structureCoords.Add(new Vector3Int(center.x, center.y, by));
                    placed.Add(center);
                }
            }
        return structureCoords;
    }

    void FillStructureFoundation(Block[,,] blocks, Vector3Int start, Vector3Int size, ChunkConfig config, Dictionary<BlockConfig,int> map, StructureConfig structure)
    {
        int sx = size.x, sy = size.y, sz = size.z;
        for (int x = 0; x < sx; x++)
            for (int z = 0; z < sz; z++)
            {
                int matIdx = structure.serializedMatrix[x + sx * z];
                if (matIdx == 0) continue;
                int wx = start.x + x, wz = start.z + z, fy = start.y - 1, li = 0;
                while (fy >= 0 && li < config.layers.Count)
                {
                    var layer = config.layers[li];
                    int gi = map[layer.bloc];
                    for (int j = 0; j < layer.thickness && fy >= 0; j++)
                    {
                        if (blocks[wx, wz, fy] == null) blocks[wx, wz, fy] = new Block(gi, layer.bloc);
                        fy--;
                    }
                    li++;
                }
                if (fy >= 0 && config.layers.Count > 0)
                {
                    var last = config.layers[^1];
                    int gi = map[last.bloc];
                    while (fy >= 0) { blocks[wx, wz, fy] = new Block(gi, last.bloc); fy--; }
                }
            }
    }

    void LogReport(List<Vector3Int> structures, Dictionary<BlockConfig,List<Vector3Int>> veins, Dictionary<BlockConfig,int> counts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<color=#00ff00><b>Chunk Report</b></color>");
        sb.AppendLine($"<b>Structures:</b> {structures.Count}");
        foreach (var s in structures) sb.AppendLine($"- ({s.x},{s.y},{s.z})");
        sb.AppendLine("<b>Ore Veins:</b>");
        foreach (var kv in veins)
        {
            sb.AppendLine($"<i>{kv.Key.name}</i>: coords {kv.Value.Count}, blocks {counts[kv.Key]}");
        }
        Debug.Log(sb.ToString());
    }
}
