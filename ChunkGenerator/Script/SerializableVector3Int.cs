using UnityEngine;

[System.Serializable]
public class SerializableVector3Int
{
    public int x;
    public int y;
    public int z;

    public SerializableVector3Int() { }

    public SerializableVector3Int(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }

    public Vector3Int ToVector3Int() => new Vector3Int(x, y, z);
    public void FromVector3Int(Vector3Int v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}
