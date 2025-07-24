using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkLayer
{
    public BlockConfig bloc;
    public int thickness;
    public List<OreConfig> ores;
}