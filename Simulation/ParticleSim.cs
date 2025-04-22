using System.Numerics;
using Ation.Common;
using System.IO;
using System.Text.Json;
using Ation.Entities;

namespace Ation.Simulation_old;

abstract class Material
{
    public Vector2 Velocity = Vector2.Zero;
    public Vector2 NetForce = Vector2.Zero;
    protected Vector2 movementRemainder = Vector2.Zero;

    public float Mass = 1.0f;
    public Raylib_cs.Color Color;
    public abstract string DisplayName { get; }

    public virtual void ApplyForce(Vector2 force) => NetForce += force;

    public virtual void Integrate(float dt)
    {
        var acceleration = NetForce / Mass;
        Velocity += acceleration * dt;
        NetForce = Vector2.Zero;
    }

    public abstract void Step(ParticleSim sim, int x, int y);
}

class Particle
{
    public Material Material;
    public bool UpdatedThisFrame;

    public Particle(Material material)
    {
        Material = material;
        UpdatedThisFrame = false;
    }
}


class Sand : Material
{
    public override string DisplayName => "Sand";
    public Sand()
    {
        Color = Raylib_cs.Color.Yellow;
        Mass = 1.0f;
    }

    public override void Step(ParticleSim sim, int x, int y)
    {
        float dt = Raylib_cs.Raylib.GetFrameTime();

        // Accumulate sub-tile movement over time
        movementRemainder += Velocity * dt;

        int moveX = (int)MathF.Floor(movementRemainder.X);
        int moveY = (int)MathF.Floor(movementRemainder.Y);

        movementRemainder.X -= moveX;
        movementRemainder.Y -= moveY;

        if (moveX != 0)
        {
            int targetX = x + Math.Sign(moveX);
            if (sim.IsEmpty(targetX, y))
            {
                sim.Swap(x, y, targetX, y);
                x = targetX;
            }
        }

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            int targetY = y + Math.Sign(moveY);
            if (!sim.IsValidCell(x, targetY)) return;

            var below = sim.Get(x, targetY);
            if (below == null || below.Material is Water)
            {
                sim.Swap(x, y, x, targetY);
                y = targetY;
            }
            else
            {
                bool canLeft = sim.IsEmpty(x - 1, y + 1) || sim.IsWater(x - 1, y + 1);
                bool canRight = sim.IsEmpty(x + 1, y + 1) || sim.IsWater(x + 1, y + 1);

                if (canLeft && canRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 1) == 0)
                        sim.Swap(x, y, x - 1, y + 1);
                    else
                        sim.Swap(x, y, x + 1, y + 1);
                }
                else if (canLeft) sim.Swap(x, y, x - 1, y + 1);
                else if (canRight) sim.Swap(x, y, x + 1, y + 1);
                else Velocity *= 0.5f;

                return;
            }
        }
    }


}

class Water : Material
{
    public override string DisplayName => "Water";
    private int dispersionRate = 5;
    private Vector2 movementRemainder = Vector2.Zero;

    public Water()
    {
        Color = Raylib_cs.Color.Blue;
        Mass = 1.0f;
    }

    public override void Step(ParticleSim sim, int x, int y)
    {
        float dt = Raylib_cs.Raylib.GetFrameTime();
        movementRemainder += Velocity * dt;

        int moveY = (int)MathF.Floor(movementRemainder.Y);
        movementRemainder.Y -= moveY;

        // Vertical falling using framerate-independent movement
        for (int i = 0; i < Math.Abs(moveY); i++)
        {
            int targetY = y + Math.Sign(moveY);
            if (sim.IsValidCell(x, targetY) && sim.IsEmpty(x, targetY))
            {
                sim.Swap(x, y, x, targetY);
                y = targetY;
            }
            else break;
        }

        // Spread horizontally (scan outward)
        for (int i = 1; i <= dispersionRate; i++)
        {
            if (sim.IsValidCell(x - i, y) && sim.IsEmpty(x - i, y))
            {
                sim.Swap(x, y, x - i, y);
                break;
            }
            else if (sim.IsValidCell(x + i, y) && sim.IsEmpty(x + i, y))
            {
                sim.Swap(x, y, x + i, y);
                break;
            }
        }
    }
}



class Steam : Material
{
    public override string DisplayName => "Steam";
    public Steam()
    {
        Color = Raylib_cs.Color.LightGray;
        Mass = 1.0f;
    }

    public override void Step(ParticleSim sim, int x, int y)
    {
        Velocity.Y += -0.2f;
        Velocity.X += Raylib_cs.Raylib.GetRandomValue(-5, 5) / 50.0f;
        Velocity.X *= 0.85f;
        Velocity.X = Math.Clamp(Velocity.X, -2.5f, 2.5f);

        int moveX = (int)MathF.Round(Velocity.X);
        int moveY = (int)MathF.Round(Velocity.Y);

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            int targetY = y - 1;
            if (!sim.IsValidCell(x, targetY)) return;

            if (sim.IsEmpty(x, targetY))
            {
                sim.Swap(x, y, x, targetY);
                y = targetY;
            }
            else
            {
                bool left = sim.IsEmpty(x - 1, y);
                bool right = sim.IsEmpty(x + 1, y);

                if (left && right)
                {
                    int dir = Raylib_cs.Raylib.GetRandomValue(0, 1) == 0 ? -1 : 1;
                    sim.Swap(x, y, x + dir, y);
                    x += dir;
                }
                else if (left)
                {
                    sim.Swap(x, y, x - 1, y);
                    x--;
                }
                else if (right)
                {
                    sim.Swap(x, y, x + 1, y);
                    x++;
                }
                else
                {
                    Velocity = Vector2.Zero;
                    break;
                }
            }
        }
    }
}
class ParticleSim
{
    private Particle?[,] grid = new Particle?[Variables.WindowHeight / Variables.PixelSize, Variables.WindowWidth / Variables.PixelSize];
    private const float Gravity = 1000.0f;

    public void Init()
    {
        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
                grid[y, x] = null;
    }

    public void Update(float dt, List<ICollider>? colliders = null)
    {
        HandleCollisions(colliders);

        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                var p = grid[y, x];
                if (p != null)
                    p.Material.ApplyForce(new Vector2(0, Gravity));
            }

        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
                grid[y, x]?.Material.Integrate(dt);

        for (int y = grid.GetLength(0) - 1; y >= 0; y--)
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                var p = grid[y, x];
                if (p == null || p.UpdatedThisFrame) continue;

                p.Material.Step(this, x, y);
                p.UpdatedThisFrame = true;
            }

        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
                if (grid[y, x] != null)
                    grid[y, x]!.UpdatedThisFrame = false;
    }

    public void SetParticle(int x, int y, Material material) => grid[y, x] = new Particle(material);

    public Particle? Get(int x, int y)
    {
        if (!IsValidCell(x, y)) return null;
        return grid[y, x];
    }

    public bool IsValidCell(int x, int y) =>
        x >= 0 && y >= 0 && y < grid.GetLength(0) && x < grid.GetLength(1);

    public bool IsEmpty(int x, int y) => IsValidCell(x, y) && grid[y, x] == null;

    public bool IsWater(int x, int y) =>
        IsValidCell(x, y) && grid[y, x]?.Material is Water;

    public void Swap(int x1, int y1, int x2, int y2)
    {
        (grid[y1, x1], grid[y2, x2]) = (grid[y2, x2], grid[y1, x1]);
    }

    public void AddParticle(Vector2 worldPos, Material material, int radius = 3)
    {
        Vector2 gridPos = Utils.WorldToGrid(worldPos);
        int centerX = (int)gridPos.X;
        int centerY = (int)gridPos.Y;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int gx = centerX + x;
                int gy = centerY + y;
                if (x * x + y * y > radius * radius) continue;
                if (!IsValidCell(gx, gy)) continue;
                if (IsEmpty(gx, gy)) SetParticle(gx, gy, CloneMaterial(material));
            }
        }
    }
    private Material CloneMaterial(Material original) =>
        original switch
        {
            Sand => new Sand(),
            Water => new Water(),
            Steam => new Steam(),
            //Solid => new Solid(),
            _ => throw new NotSupportedException("Unknown material type")
        };

    public void ClearParticles(Vector2 worldPos, int radius = 3)
    {
        Vector2 gridPos = Utils.WorldToGrid(worldPos);
        int centerX = (int)gridPos.X;
        int centerY = (int)gridPos.Y;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int gx = centerX + x;
                int gy = centerY + y;
                if (x * x + y * y > radius * radius) continue;
                if (!IsValidCell(gx, gy)) continue;
                grid[gy, gx] = null;
            }
        }
    }


    public void Render()
    {
        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                var p = grid[y, x];
                if (p == null) continue;

                Vector2 pos = Utils.GridToWorld(new Vector2(x, y));
                Raylib_cs.Raylib.DrawRectangle((int)pos.X, (int)pos.Y, Variables.PixelSize, Variables.PixelSize, p.Material.Color);
            }
    }

    private void HandleCollisions(List<ICollider>? colliders)
    {
        if (colliders == null) return;

        foreach (var collider in colliders)
        {
            var bounds = collider.GetBounds();
            int minX = (int)(bounds.X / Variables.PixelSize);
            int minY = (int)(bounds.Y / Variables.PixelSize);
            int maxX = (int)((bounds.X + bounds.Width) / Variables.PixelSize);
            int maxY = (int)((bounds.Y + bounds.Height) / Variables.PixelSize);

            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    if (!IsValidCell(x, y)) continue;

                    if (grid[y, x]?.Material is Sand && IsEmpty(x, y - 1))
                        Swap(x, y, x, y - 1);
                }
        }
    }

    public bool IsOccupied(Vector2 worldPosition)
    {
        Vector2 gridPos = Utils.WorldToGrid(worldPosition);
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        return IsValidCell(x, y) && grid[y, x] != null;
    }
    public int CountParticles()
    {
        int count = 0;
        for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
                if (grid[y, x] != null)
                    count++;
        return count;
    }

}
