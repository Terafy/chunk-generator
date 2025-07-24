using UnityEngine;

[CreateAssetMenu(menuName = "Chunks/Block Config")]
public class BlockConfig : ScriptableObject
{
    public Material material;
    public int strength;
    public bool IsTransparent = false;
}
