using System.Numerics;
using Ation.Common;

namespace Ation.ParticleSimulation;

enum ParticleType
{
    Empty,
    Sand,
    Water,
    Steam,
    Solid
}

struct Particle
{
    public ParticleType Type;
    public Raylib_cs.Color Color;
    public Vector2 Velocity; // Now has velocity
}

class ParticleSim
{
    private Particle[,] grid = new Particle[
        (int)Variables.WindowHeight / Variables.PixelSize,
        (int)Variables.WindowWidth / Variables.PixelSize
    ];

    private const float Gravity = 1.0f; // Acceleration per frame

    public ParticleSim() { }

    public void Init()
    {
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                grid[y, x] = new Particle
                {
                    Type = ParticleType.Empty,
                    Color = new Raylib_cs.Color(0, 0, 0, 0),
                    Velocity = Vector2.Zero
                };
            }
        }
    }

    public void Update(float dt)
    {
        for (int y = grid.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = grid.GetLength(1) - 1; x >= 0; x--)
            {
                switch (grid[y, x].Type)
                {
                    case ParticleType.Sand:
                        processSand(x, y);
                        break;
                    case ParticleType.Water:
                        processWater(x, y);
                        break;
                    default:
                        break;
                }
            }
        }
    }


    private void processSand(int x, int y)
    {
        if (grid[y, x].Type == ParticleType.Empty) return;

        // Apply gravity
        grid[y, x].Velocity.Y += Gravity;

        // Get how many cells to move based on velocity
        int moveX = (int)MathF.Round(grid[y, x].Velocity.X);
        int moveY = (int)MathF.Round(grid[y, x].Velocity.Y);

        // Try to move down by moveY cells (one at a time)
        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            if (y + 1 >= grid.GetLength(0)) return;

            if (grid[y + 1, x].Type == ParticleType.Empty)
            {
                SwapCells(x, y, x, y + 1);
                y += 1;
            }
            else
            {
                bool canMoveLeft = (x > 0 && grid[y + 1, x - 1].Type == ParticleType.Empty);
                bool canMoveRight = (x < grid.GetLength(1) - 1 && grid[y + 1, x + 1].Type == ParticleType.Empty);

                if (canMoveLeft && canMoveRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 1) == 0)
                    {
                        SwapCells(x, y, x - 1, y + 1);
                        x -= 1;
                        y += 1;
                    }
                    else
                    {
                        SwapCells(x, y, x + 1, y + 1);
                        x += 1;
                        y += 1;
                    }
                }
                else if (canMoveLeft)
                {
                    SwapCells(x, y, x - 1, y + 1);
                    x -= 1;
                    y += 1;
                }
                else if (canMoveRight)
                {
                    SwapCells(x, y, x + 1, y + 1);
                    x += 1;
                    y += 1;
                }
                else
                {
                    // Collision - reduce velocity a bit
                    grid[y, x].Velocity.Y *= 0.5f;
                    break;
                }
            }
        }
    }

    private void processWater(int x, int y)
    {
        if (grid[y, x].Type != ParticleType.Water) return;

        grid[y, x].Velocity.Y += Gravity;

        int moveX = (int)MathF.Round(grid[y, x].Velocity.X);
        int moveY = (int)MathF.Round(grid[y, x].Velocity.Y);

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            if (y + 1 >= grid.GetLength(0)) return;

            if (grid[y + 1, x].Type == ParticleType.Empty)
            {
                SwapCells(x, y, x, y + 1);
                y += 1;
            }
            else
            {
                bool canMoveLeft = (x > 0 && grid[y, x - 1].Type == ParticleType.Empty);
                bool canMoveRight = (x < grid.GetLength(1) - 1 && grid[y, x + 1].Type == ParticleType.Empty);

                if (canMoveLeft && canMoveRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 1) == 0)
                    {
                        SwapCells(x, y, x - 1, y);
                        x -= 1;
                    }
                    else
                    {
                        SwapCells(x, y, x + 1, y);
                        x += 1;
                    }
                }
                else if (canMoveLeft)
                {
                    SwapCells(x, y, x - 1, y);
                    x -= 1;
                }
                else if (canMoveRight)
                {
                    SwapCells(x, y, x + 1, y);
                    x += 1;
                }
                else
                {
                    // Fully blocked
                    grid[y, x].Velocity = Vector2.Zero;
                    break;
                }
            }
        }
    }

    private void SwapCells(int origX, int origY, int x, int y)
    {
        Particle temp = grid[y, x];
        grid[y, x] = grid[origY, origX];
        grid[origY, origX] = temp;
    }

    public void Render()
    {
        int gridHeight = grid.GetLength(0);
        int gridWidth = grid.GetLength(1);

        // Raylib_cs.Raylib.BeginDrawing();
        // Raylib_cs.Raylib.ClearBackground(Raylib_cs.Color.LightGray);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Particle particle = grid[y, x];
                if (particle.Type == ParticleType.Empty) continue;

                Vector2 pos = Utils.GridToWorld(new Vector2(x, y));

                if (particle.Color.A > 0)
                {
                    Raylib_cs.Raylib.DrawRectangle(
                        (int)pos.X,
                        (int)pos.Y,
                        Variables.PixelSize,
                        Variables.PixelSize,
                        particle.Color
                    );
                }
            }
        }

        Raylib_cs.Raylib.DrawText($"FPS: {Raylib_cs.Raylib.GetFPS()}", 12, 12, 20, Raylib_cs.Color.Black);
        Raylib_cs.Raylib.DrawText($"# of particles: {CountParticles()}", 12, 35, 20, Raylib_cs.Color.Black);


        // Raylib_cs.Raylib.EndDrawing();
    }

    public void AddParticle(Vector2 position, ParticleType type, int radius = 3)
    {
        Vector2 gridPos = Utils.WorldToGrid(position);

        int centerX = (int)gridPos.X;
        int centerY = (int)gridPos.Y;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int spawnX = centerX + x;
                int spawnY = centerY + y;

                if (x * x + y * y > radius * radius) continue;

                if (Utils.IsInGridBounds(new Vector2(spawnX, spawnY)) && grid[spawnY, spawnX].Type == ParticleType.Empty)
                {
                    Raylib_cs.Color color = type switch
                    {
                        ParticleType.Sand => Raylib_cs.Color.Yellow,
                        ParticleType.Water => Raylib_cs.Color.Blue,
                        _ => new Raylib_cs.Color(255, 255, 255, 255) // fallback
                    };

                    grid[spawnY, spawnX] = new Particle
                    {
                        Type = type,
                        Color = color,
                        Velocity = new Vector2(Raylib_cs.Raylib.GetRandomValue(-1, 1), 0)
                    };
                }
            }
        }
    }




    public int CountParticles()
    {
        int count = 0;
        foreach (var cell in grid)
        {
            if (cell.Type != ParticleType.Empty)
                count++;
        }
        return count;
    }
}
