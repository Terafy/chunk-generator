using UnityEngine;

[CreateAssetMenu(menuName = "Chunks/Structure Config")]
public class StructureConfig : ScriptableObject
{
    public SerializableVector3Int sizeRaw;
    public SerializableVector3Int anchorRaw;
    public int[] serializedMatrix;
    public BlockConfig[] blockTemplates;
    public bool flattenGroundUnderStructure = true;

    public Vector3Int size => sizeRaw.ToVector3Int();
    public Vector3Int anchor => anchorRaw.ToVector3Int();

    public int GetIndex(int x, int y, int z) => x + size.x * (y + size.y * z);
    public int GetValue(int x, int y, int z) => serializedMatrix[GetIndex(x, y, z)];
    public void SetValue(int x, int y, int z, int value) => serializedMatrix[GetIndex(x, y, z)] = value;
}
