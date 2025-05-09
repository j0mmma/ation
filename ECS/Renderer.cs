using System.Numerics;
using Raylib_cs;
using Ation.GameWorld;
using Ation.Common;

namespace Ation.Entities;


public class Renderer
{
    private readonly EntityManager em;
    private readonly World world;

    public bool DebugDrawColliders = true;
    public bool DebugDrawPositions = false;
    public bool DebugDrawHealthBars = true;

    public Renderer(EntityManager em, World world)
    {
        this.em = em;
        this.world = world;
    }

    public void Render()
    {
        DrawSprites();

        if (DebugDrawColliders) DrawColliders();
        if (DebugDrawPositions) DrawEntityPositions();
        if (DebugDrawHealthBars) DrawHealthBars();

        //DrawHUD(); // Always drawn
    }

    private void DrawSprites()
    {
        foreach (var (e, renderable) in em.GetAll<RenderableComponent>())
        {
            if (!em.TryGetComponent(e, out TransformComponent transform)) continue;

            float px = transform.Position.X * Variables.PixelSize + renderable.Offset.X;
            float py = transform.Position.Y * Variables.PixelSize + renderable.Offset.Y;

            float width = renderable.Source.Width * renderable.Scale;
            float height = renderable.Source.Height * renderable.Scale;

            Rectangle dest = new Rectangle(px, py, width, height);
            Vector2 origin = new Vector2(width / 2f, height); // pivot at bottom-center

            Raylib.DrawTexturePro(
                renderable.Texture,
                renderable.Source,
                dest,
                origin,
                0f,
                renderable.Tint
            );

            // Draw red outline around the sprite
            var outline = new Rectangle(dest.X - origin.X, dest.Y - origin.Y, dest.Width, dest.Height);
            Raylib.DrawRectangleLinesEx(outline, 1, Color.Red);
        }
    }





    private void DrawColliders()
    {
        foreach (var (e, collider) in em.GetAll<ColliderComponent>())
        {
            if (!em.TryGetComponent(e, out TransformComponent pos)) continue;

            float px = (pos.Position.X + collider.Offset.X) * Variables.PixelSize;
            float py = (pos.Position.Y + collider.Offset.Y) * Variables.PixelSize;
            float width = collider.Size.X * Variables.PixelSize;
            float height = collider.Size.Y * Variables.PixelSize;

            var rect = new Rectangle(px, py, width, height);
            Raylib.DrawRectangleLinesEx(rect, 1, Color.Green);
        }
    }


    private void DrawEntityPositions()
    {
        foreach (var (e, pos) in em.GetAll<TransformComponent>())
        {
            Raylib.DrawText($"({pos.Position.X:0},{pos.Position.Y:0})", (int)(pos.Position.X + 5), (int)(pos.Position.Y - 10), 10, Color.Yellow);
        }
    }

    private void DrawHealthBars()
    {
        // If you add HealthComponent later
    }

    private void RenderHUD()
    {
        // HUD elements like brush size, material, FPS, etc.
    }
}



public class SpriteSheet
{
    public Texture2D Texture { get; }
    private readonly Dictionary<string, Rectangle> regions;

    public SpriteSheet(Texture2D texture)
    {
        Texture = texture;
        regions = new();
    }

    public void AddRegion(string name, int x, int y, int width, int height)
    {
        regions[name] = new Rectangle(x, y, width, height);
    }

    public Rectangle GetRegion(string name)
    {
        return regions.TryGetValue(name, out var rect)
            ? rect
            : throw new Exception($"Sprite '{name}' not found.");
    }
}

public static class TextureManager
{
    private static readonly Dictionary<string, SpriteSheet> sheets = new();

    public static void LoadSheet(string name, string path, int spriteWidth, int spriteHeight, string[] spriteNames)
    {
        Texture2D texture = Raylib.LoadTexture(path);
        var sheet = new SpriteSheet(texture);

        for (int i = 0; i < spriteNames.Length; i++)
        {
            int x = i * spriteWidth;
            sheet.AddRegion(spriteNames[i], x, 0, spriteWidth, spriteHeight);
        }

        sheets[name] = sheet;
    }
    public static bool TryGetSheet(string name, out SpriteSheet sheet) =>
    sheets.TryGetValue(name, out sheet!);

    public static SpriteSheet GetSheet(string name) => sheets[name];
}
