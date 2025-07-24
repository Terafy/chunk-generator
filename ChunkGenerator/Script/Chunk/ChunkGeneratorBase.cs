using UnityEngine;

public abstract class ChunkGenerator : ScriptableObject
{
    public abstract Block[,,] Generate(ChunkConfig config, out int maxHeight);
}
