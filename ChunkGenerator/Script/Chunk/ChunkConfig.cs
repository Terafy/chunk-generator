using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chunks/Chunk Config")]
public class ChunkConfig : ScriptableObject
{
    public int Width = 16;
    public int Length = 16;
    public List<ChunkLayer> layers;
    public List<ChunkStructure> structures;
    public List<BlockConfig> globalBlockConfigs;
    public ChunkGenerator generator;
}
