using System.Numerics;
using Ation.Common;

namespace Ation.ParticleSimulation;

enum ParticleType
{
    Empty,
    Sand,
    Water,
    Steam,
    Solid,
    Eraser // <- new!
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
                    case ParticleType.Steam:
                        processSteam(x, y);
                        break;
                    case ParticleType.Solid:
                        break;
                }

            }
        }
    }



    private void processSand(int x, int y)
    {
        if (grid[y, x].Type != ParticleType.Sand) return;

        // Apply gravity
        grid[y, x].Velocity.Y += Gravity;

        int moveX = (int)MathF.Round(grid[y, x].Velocity.X);
        int moveY = (int)MathF.Round(grid[y, x].Velocity.Y);

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            if (y + 1 >= grid.GetLength(0)) return;

            Particle below = grid[y + 1, x];

            if (below.Type == ParticleType.Empty)
            {
                SwapCells(x, y, x, y + 1);
                y += 1;
            }
            else if (below.Type == ParticleType.Water)
            {
                // Try to push water sideways first
                bool canPushLeft = (x > 0 && grid[y + 1, x - 1].Type == ParticleType.Empty);
                bool canPushRight = (x < grid.GetLength(1) - 1 && grid[y + 1, x + 1].Type == ParticleType.Empty);

                if (canPushLeft && canPushRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 1) == 0)
                    {
                        SwapCells(x, y + 1, x - 1, y + 1); // push water left
                        SwapCells(x, y, x, y + 1);          // fall into water spot
                        x -= 1;
                        y += 1;
                    }
                    else
                    {
                        SwapCells(x, y + 1, x + 1, y + 1); // push water right
                        SwapCells(x, y, x, y + 1);
                        x += 1;
                        y += 1;
                    }
                }
                else if (canPushLeft)
                {
                    SwapCells(x, y + 1, x - 1, y + 1);
                    SwapCells(x, y, x, y + 1);
                    x -= 1;
                    y += 1;
                }
                else if (canPushRight)
                {
                    SwapCells(x, y + 1, x + 1, y + 1);
                    SwapCells(x, y, x, y + 1);
                    x += 1;
                    y += 1;
                }
                else
                {
                    // Fully blocked → only now swap vertically
                    SwapCells(x, y, x, y + 1);
                    y += 1;
                }
            }
            else
            {
                bool canMoveLeft = (x > 0 && (grid[y + 1, x - 1].Type == ParticleType.Empty || grid[y + 1, x - 1].Type == ParticleType.Water));
                bool canMoveRight = (x < grid.GetLength(1) - 1 && (grid[y + 1, x + 1].Type == ParticleType.Empty || grid[y + 1, x + 1].Type == ParticleType.Water));

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
                    // Full collision → reduce vertical velocity
                    grid[y, x].Velocity.Y *= 0.5f;
                    break;
                }
            }
        }
    }

    private void processWater(int x, int y)
    {
        if (grid[y, x].Type != ParticleType.Water) return;

        float horizontalDamping = 0.85f; // Dampen sideways movement slowly
        float maxHorizontalSpeed = 2.5f; // Cap max speed sideways

        grid[y, x].Velocity.Y += Gravity;

        // Dampen horizontal velocity each frame
        grid[y, x].Velocity.X *= horizontalDamping;

        // Clamp horizontal speed to avoid crazy drifting
        grid[y, x].Velocity.X = Math.Clamp(grid[y, x].Velocity.X, -maxHorizontalSpeed, maxHorizontalSpeed);

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
                    grid[y, x].Velocity = Vector2.Zero;
                    break;
                }
            }
        }
    }

    private void processSteam(int x, int y)
    {
        if (grid[y, x].Type != ParticleType.Steam) return;

        float horizontalDamping = 0.85f;
        float maxHorizontalSpeed = 2.5f;
        float liftForce = -0.2f; // Negative = upward

        grid[y, x].Velocity.Y += liftForce; // Apply upward force

        // Add a tiny random horizontal drift for puffiness
        grid[y, x].Velocity.X += Raylib_cs.Raylib.GetRandomValue(-5, 5) / 50.0f;

        // Dampen horizontal velocity each frame
        grid[y, x].Velocity.X *= horizontalDamping;

        // Clamp horizontal drift speed
        grid[y, x].Velocity.X = Math.Clamp(grid[y, x].Velocity.X, -maxHorizontalSpeed, maxHorizontalSpeed);

        int moveX = (int)MathF.Round(grid[y, x].Velocity.X);
        int moveY = (int)MathF.Round(grid[y, x].Velocity.Y);

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            if (y - 1 < 0) return; // Top of the grid

            if (grid[y - 1, x].Type == ParticleType.Empty)
            {
                SwapCells(x, y, x, y - 1);
                y -= 1;
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

                if (x * x + y * y > radius * radius) continue; // circular brush

                if (Utils.IsInGridBounds(new Vector2(spawnX, spawnY)))
                {
                    if (type == ParticleType.Eraser)
                    {
                        // Clear any non-empty particle
                        if (grid[spawnY, spawnX].Type != ParticleType.Empty)
                        {
                            grid[spawnY, spawnX] = new Particle
                            {
                                Type = ParticleType.Empty,
                                Color = new Raylib_cs.Color(0, 0, 0, 0),
                                Velocity = Vector2.Zero
                            };
                        }
                    }
                    else
                    {
                        // Only spawn new particle if cell is empty
                        if (grid[spawnY, spawnX].Type == ParticleType.Empty)
                        {
                            Raylib_cs.Color color = type switch
                            {
                                ParticleType.Sand => Raylib_cs.Color.Yellow,
                                ParticleType.Water => Raylib_cs.Color.Blue,
                                ParticleType.Solid => Raylib_cs.Color.DarkGray,
                                ParticleType.Steam => Raylib_cs.Color.Gray,
                                _ => new Raylib_cs.Color(255, 255, 255, 255)
                            };

                            Vector2 randomVelocity = type switch
                            {
                                ParticleType.Water => new Vector2(Raylib_cs.Raylib.GetRandomValue(-10, 10) / 5.0f, 0),
                                ParticleType.Sand => new Vector2(Raylib_cs.Raylib.GetRandomValue(-1, 1), 0),
                                ParticleType.Steam => new Vector2(
                                    Raylib_cs.Raylib.GetRandomValue(-30, 30) / 10.0f,
                                    Raylib_cs.Raylib.GetRandomValue(-5, -1) / 10.0f
                                ),
                                _ => Vector2.Zero
                            };

                            grid[spawnY, spawnX] = new Particle
                            {
                                Type = type,
                                Color = color,
                                Velocity = randomVelocity
                            };
                        }
                    }
                }
            }
        }
    }





    public bool IsOccupied(Vector2 worldPosition)
    {
        Vector2 gridPos = Utils.WorldToGrid(worldPosition);

        if (!Utils.IsInGridBounds(gridPos))
            return false;

        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;

        return grid[y, x].Type != ParticleType.Empty;
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
