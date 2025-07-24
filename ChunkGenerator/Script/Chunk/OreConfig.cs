using UnityEngine;

[System.Serializable]
public class OreConfig
{
    public BlockConfig block;
    [Range(0f, 1f)] public float chance = 0.01f;
    public int minTotalOres = 1;
    public int maxTotalOres = 3;
    public int clusterSize = 4;
    public float noiseScale = 8f;
}