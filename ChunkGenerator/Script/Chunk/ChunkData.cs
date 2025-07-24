public class ChunkData
{
    public int Width { get; }
    public int Height { get; }
    public int Length { get; }

    public Block[,,] Blocks { get; }
    public ChunkConfig Config { get; }

    public ChunkData(ChunkConfig config)
    {
        Config = config;
        Width = config.Width;
        Length = config.Length;
        Blocks = config.generator.Generate(config, out int mh);
        Height = mh;
    }
}
