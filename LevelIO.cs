using System.Text.Json;
using Ation.Simulation;
using Ation.GameWorld;
using Ation.Common;

public class SavedMaterial
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? Type { get; set; }
}

public static class LevelIO
{
    public static void Save(string filePath, World world)
    {
        var materials = new List<SavedMaterial>();

        foreach (var chunk in world.GetAllChunks())
        {
            var (chunkX, chunkY) = (chunk.ChunkX, chunk.ChunkY);
            var (w, h) = chunk.Size;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var mat = chunk.Grid.Get(x, y);
                    if (mat == null) continue;

                    int worldX = chunkX * w + x;
                    int worldY = chunkY * h + y;

                    materials.Add(new SavedMaterial
                    {
                        X = worldX,
                        Y = worldY,
                        Type = mat.Type.ToString()
                    });
                }
            }
        }

        var json = JsonSerializer.Serialize(materials);
        File.WriteAllText(filePath, json);
    }

    public static void Load(string filePath, World world)
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        var materials = JsonSerializer.Deserialize<List<SavedMaterial>>(json);

        foreach (var mat in materials)
        {
            if (Enum.TryParse(mat.Type, out MaterialType type))
            {
                var material = MaterialFactory.Create(type, Utils.GridToWorld(new(mat.X, mat.Y)));
                world.Set(mat.X, mat.Y, material);
            }
        }
    }
}
