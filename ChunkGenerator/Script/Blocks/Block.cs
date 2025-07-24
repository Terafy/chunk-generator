using UnityEngine;
public class Block : IHitByPlayer
{
    public BlockConfig Config;
    public int CurrentHealth;
    public int LayerIndex;
    public bool IsDestroyed => CurrentHealth <= 0;
    public bool IsTransparent = false;

    public Block(int layerIndex, BlockConfig config, bool isTransparent = false)
    {
        LayerIndex = layerIndex;
        Config = config;
        CurrentHealth = config.strength;
        IsTransparent = isTransparent;
    }

    public void Hit(int damage)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }
}
