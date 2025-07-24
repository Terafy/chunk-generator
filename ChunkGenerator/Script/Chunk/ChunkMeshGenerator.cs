using UnityEngine;
using System.Collections.Generic;

public static class ChunkMeshGenerator
{
    public static Mesh GenerateMesh(ChunkData chunk, HashSet<Vector3Int> excludePositions = null)
    {
        int total = chunk.Config.globalBlockConfigs.Count;

        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs = new List<Vector2>();
        var uv2 = new List<Vector2>();
        var submesh = new List<int>[total];
        for (int i = 0; i < total; i++) submesh[i] = new List<int>();

        var camera = Camera.main;
        Vector3 chunkCenter = new Vector3(chunk.Width, chunk.Height, chunk.Length) * 0.5f;
        Vector3 viewDir = (chunkCenter - camera.transform.position).normalized;

        Vector3Int[] directions = {
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.down
        };

        List<Vector3Int> visibleDirs = new();
        foreach (var dir in directions)
        {
            if (dir == Vector3Int.down) continue;
            float dot = Vector3.Dot(viewDir, dir);
            if (dot >= -0.5f) visibleDirs.Add(dir);
        }

        for (int x = 0; x < chunk.Width; x++)
        for (int y = 0; y < chunk.Height; y++)
        for (int z = 0; z < chunk.Length; z++)
        {
            if (excludePositions != null && excludePositions.Contains(new Vector3Int(x, y, z))) continue;

            var block = chunk.Blocks[x, z, y];
            if (block == null || block.IsDestroyed) continue;
            var pos = new Vector3(x, y, z);

            foreach (var dir in visibleDirs)
                TryAddFace(chunk, x, y, z, pos, dir, block, verts, norms, uvs, uv2, submesh);
        }

        var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, uv2);
        mesh.subMeshCount = total;
        for (int i = 0; i < total; i++)
            mesh.SetTriangles(submesh[i], i);

        return mesh;
    }

    static void TryAddFace(ChunkData c, int x, int y, int z, Vector3 pos, Vector3Int n, Block block,
        List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs, List<Vector2> uv2, List<int>[] submesh)
    {
        int nx = x + n.x, ny = y + n.y, nz = z + n.z;
        bool outside = nx < 0 || ny < 0 || nz < 0 || nx >= c.Width || ny >= c.Height || nz >= c.Length;
        Block neighbor = !outside ? c.Blocks[nx, nz, ny] : null;
        if (!outside && neighbor != null && !neighbor.IsDestroyed)
        {
            bool nbT = neighbor.Config.IsTransparent;
            bool curT = block.Config.IsTransparent;
            if (!nbT || (nbT && curT)) return;
        }

        Vector3[] fv = GetFaceVertices(pos, n);
        Vector2[] baseUV = {
            new Vector2(0,0), new Vector2(1,0),
            new Vector2(1,1), new Vector2(0,1)
        };
        float prog = block.CurrentHealth / (float)block.Config.strength;
        int start = verts.Count;
        for (int i = 0; i < 4; i++)
        {
            verts.Add(fv[i]);
            norms.Add(n);
            uvs.Add(baseUV[i]);
            uv2.Add(new Vector2(prog, 0));
        }
        submesh[block.LayerIndex].AddRange(new[] { start, start+1, start+2, start+2, start+3, start });
    }

    static Vector3[] GetFaceVertices(Vector3 p, Vector3Int n)
    {
        if (n == Vector3Int.forward) return new[] { p + new Vector3(0,0,1), p + new Vector3(1,0,1), p + new Vector3(1,1,1), p + new Vector3(0,1,1) };
        if (n == Vector3Int.back)    return new[] { p + new Vector3(1,0,0), p + new Vector3(0,0,0), p + new Vector3(0,1,0), p + new Vector3(1,1,0) };
        if (n == Vector3Int.left)    return new[] { p + new Vector3(0,0,0), p + new Vector3(0,0,1), p + new Vector3(0,1,1), p + new Vector3(0,1,0) };
        if (n == Vector3Int.right)   return new[] { p + new Vector3(1,0,1), p + new Vector3(1,0,0), p + new Vector3(1,1,0), p + new Vector3(1,1,1) };
        if (n == Vector3Int.up)      return new[] { p + new Vector3(0,1,1), p + new Vector3(1,1,1), p + new Vector3(1,1,0), p + new Vector3(0,1,0) };
        return                          new[] { p + new Vector3(0,0,0), p + new Vector3(1,0,0), p + new Vector3(1,0,1), p + new Vector3(0,0,1) };
    }
}
