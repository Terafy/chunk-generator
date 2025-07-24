using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private ChunkData chunkData;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    public void Initialize(ChunkConfig config)
    {
        chunkData = new ChunkData(config);
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void GenerateVisuals()
    {
        var mesh = ChunkMeshGenerator.GenerateMesh(chunkData);
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider.sharedMesh = mesh;

        var baseMaterials = chunkData.Config.globalBlockConfigs.Select(bc => bc.material).ToArray();
        meshRenderer.materials = baseMaterials;
    }

    public void ClearVisuals()
    {
        GetComponent<MeshFilter>().mesh = null;
        meshCollider.sharedMesh = null;
        meshRenderer.materials = null;
    }

    public bool HitBlockAtWorldPos(Vector3 worldPos, int damage)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos) - Vector3.one * 0.01f;
        int x = Mathf.FloorToInt(local.x), y = Mathf.FloorToInt(local.y), z = Mathf.FloorToInt(local.z);

        if (x < 0 || y < 0 || z < 0 || x >= chunkData.Width || y >= chunkData.Height || z >= chunkData.Length)
            return false;

        var block = chunkData.Blocks[x, z, y];
        if (block == null || block.IsDestroyed) return false;

        block.Hit(damage);
        GenerateVisuals();
        return block.IsDestroyed;
    }
}
