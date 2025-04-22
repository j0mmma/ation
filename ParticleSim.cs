using System.Numerics;
using Ation.Common;
using System.IO;
using System.Text.Json;
using Ation.Entities;
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
    public Vector2 Velocity;
    public Vector2 NetForce;
    public float Mass;
}
class Area
{
    public string type { get; set; }
    public int startX { get; set; }
    public int startY { get; set; }
    public int endX { get; set; }
    public int endY { get; set; }
}
class ParticleSim
{
    private Particle[,] grid = new Particle[
        (int)Variables.WindowHeight / Variables.PixelSize,
        (int)Variables.WindowWidth / Variables.PixelSize
    ];

    private const float Gravity = 10.0f; // Acceleration per frame
    private readonly Vector2 BaseWind = new Vector2(20f, 0); // adjust as needed
    private const int MaxWindDepth = 6; // deeper than this → no wind effect


    private Vector2 previousMousePos = Vector2.Zero;
    private Vector2 mouseVelocity = Vector2.Zero;
    public ParticleSim() { }

    public void LoadTestLevel(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"Level file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        var areas = JsonSerializer.Deserialize<List<Area>>(json);

        if (areas == null)
        {
            Console.WriteLine("Invalid level data.");
            return;
        }

        foreach (var area in areas)
        {
            ParticleType particleType = area.type.ToLower() switch
            {
                "solid" => ParticleType.Solid,
                "water" => ParticleType.Water,
                "sand" => ParticleType.Sand,
                "steam" => ParticleType.Steam,
                _ => ParticleType.Empty,
            };

            for (int y = area.startY; y <= area.endY; y++)
            {
                for (int x = area.startX; x <= area.endX; x++)
                {
                    if (!Utils.IsInGridBounds(new Vector2(x, y)))
                        continue;

                    grid[y, x] = new Particle
                    {
                        Type = particleType,
                        Color = particleType switch
                        {
                            ParticleType.Solid => Raylib_cs.Color.DarkGray,
                            ParticleType.Water => Raylib_cs.Color.Blue,
                            ParticleType.Sand => Raylib_cs.Color.Yellow,
                            ParticleType.Steam => Raylib_cs.Color.LightGray,
                            _ => Raylib_cs.Color.White
                        },
                        Velocity = Vector2.Zero,
                        NetForce = Vector2.Zero,
                        Mass = 1.0f
                    };
                }
            }
        }
    }



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
                    Velocity = Vector2.Zero,
                    NetForce = Vector2.Zero,
                    Mass = 1.0f,
                };
            }
        }

        // Load level after clearing the grid
        //LoadTestLevel("levels/test.json"); // <- replace with actual path or make it a param
    }

    private void UpdateMouseVelocity(float dt)
    {
        Vector2 currentMouse = new Vector2(Raylib_cs.Raylib.GetMouseX(), Raylib_cs.Raylib.GetMouseY());
        mouseVelocity = (currentMouse - previousMousePos) / dt;
        previousMousePos = currentMouse;
    }


    public void Update(float dt, List<ICollider> colliders = null)
    {
        HandleCollisions(colliders);

        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                switch (grid[y, x].Type)
                {
                    case ParticleType.Sand:
                    case ParticleType.Water:
                        ApplyForce(x, y, new Vector2(0, Gravity)); // Apply gravity to both
                        break;
                }
            }
        }

        IntegrateForces(dt);

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
                }
            }
        }
    }






    private void processSand(int x, int y)
    {
        ref Particle p = ref grid[y, x];
        if (p.Type != ParticleType.Sand) return;

        int moveX = (int)MathF.Round(p.Velocity.X);
        int moveY = (int)MathF.Round(p.Velocity.Y);

        // Horizontal drift
        if (moveX != 0)
        {
            int targetX = x + Math.Sign(moveX);
            if (targetX >= 0 && targetX < grid.GetLength(1) &&
                grid[y, targetX].Type == ParticleType.Empty)
            {
                SwapCells(x, y, targetX, y);
                x = targetX;
            }
        }

        // Vertical fall
        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            int targetY = y + Math.Sign(moveY);
            if (targetY < 0 || targetY >= grid.GetLength(0)) return;

            ParticleType belowType = grid[targetY, x].Type;

            if (belowType == ParticleType.Empty || belowType == ParticleType.Water)
            {
                SwapCells(x, y, x, targetY);
                y = targetY;
            }
            else
            {
                // Diagonal fallback if blocked
                bool canLeft = x > 0 && grid[y + 1, x - 1].Type is ParticleType.Empty or ParticleType.Water;
                bool canRight = x < grid.GetLength(1) - 1 && grid[y + 1, x + 1].Type is ParticleType.Empty or ParticleType.Water;

                if (canLeft && canRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 1) == 0)
                        SwapCells(x, y, x - 1, y + 1);
                    else
                        SwapCells(x, y, x + 1, y + 1);
                }
                else if (canLeft)
                    SwapCells(x, y, x - 1, y + 1);
                else if (canRight)
                    SwapCells(x, y, x + 1, y + 1);
                else
                {
                    p.Velocity *= 0.5f;
                    break;
                }

                return;
            }
        }
    }






    private void processWater(int x, int y)
    {
        ref Particle p = ref grid[y, x];
        if (p.Type != ParticleType.Water) return;

        float horizontalDamping = 0.85f;
        float maxHorizontalSpeed = 5.5f;

        // Apply gravity as force

        // Dampen and clamp horizontal velocity (simulate water resistance)
        p.Velocity.X *= horizontalDamping;
        p.Velocity.X = Math.Clamp(p.Velocity.X, -maxHorizontalSpeed, maxHorizontalSpeed);

        int moveX = (int)MathF.Round(p.Velocity.X);
        int moveY = (int)MathF.Round(p.Velocity.Y);

        for (int step = 0; step < Math.Abs(moveY); step++)
        {
            int targetY = y + 1;
            if (targetY >= grid.GetLength(0)) return;

            if (grid[targetY, x].Type == ParticleType.Empty)
            {
                SwapCells(x, y, x, targetY);
                y = targetY;
            }
            else
            {
                bool canMoveLeft = x > 0 && grid[y, x - 1].Type == ParticleType.Empty;
                bool canMoveRight = x < grid.GetLength(1) - 1 && grid[y, x + 1].Type == ParticleType.Empty;

                if (canMoveLeft && canMoveRight)
                {
                    int dir = Raylib_cs.Raylib.GetRandomValue(0, 1) == 0 ? -1 : 1;
                    SwapCells(x, y, x + dir, y);
                    x += dir;
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
                    p.Velocity = Vector2.Zero;
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


    private void HandleCollisions(List<ICollider> colliders)
    {
        if (colliders == null) return;

        foreach (var collider in colliders)
        {
            Raylib_cs.Rectangle bounds = collider.GetBounds();
            Vector2 velocity = collider.GetVelocity();

            int minX = (int)(bounds.X / Variables.PixelSize);
            int minY = (int)(bounds.Y / Variables.PixelSize);
            int maxX = (int)((bounds.X + bounds.Width) / Variables.PixelSize);
            int maxY = (int)((bounds.Y + bounds.Height) / Variables.PixelSize);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!Utils.IsInGridBounds(new Vector2(x, y))) continue;

                    // Example: push sand out of collider area
                    if (grid[y, x].Type == ParticleType.Sand)
                    {
                        int pushY = y - 1;
                        if (Utils.IsInGridBounds(new Vector2(x, pushY)) && grid[pushY, x].Type == ParticleType.Empty)
                        {
                            SwapCells(x, y, x, pushY);
                        }
                    }
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
                if (!Utils.IsInGridBounds(new Vector2(spawnX, spawnY))) continue;

                if (type == ParticleType.Eraser)
                {
                    grid[spawnY, spawnX] = new Particle
                    {
                        Type = ParticleType.Empty,
                        Color = new Raylib_cs.Color(0, 0, 0, 0),
                        Velocity = Vector2.Zero,
                        NetForce = Vector2.Zero,
                        Mass = 1.0f,
                    };
                }
                else if (grid[spawnY, spawnX].Type == ParticleType.Empty)
                {
                    Raylib_cs.Color color = type switch
                    {
                        ParticleType.Sand => Raylib_cs.Color.Yellow,
                        ParticleType.Water => Raylib_cs.Color.Blue,
                        ParticleType.Solid => Raylib_cs.Color.DarkGray,
                        ParticleType.Steam => Raylib_cs.Color.Gray,
                        _ => new Raylib_cs.Color(255, 255, 255, 255)
                    };

                    Vector2 worldPos = Utils.GridToWorld(new Vector2(spawnX, spawnY)); // initial sub-pixel pos

                    grid[spawnY, spawnX] = new Particle
                    {
                        Type = type,
                        Color = color,
                        Velocity = Vector2.Zero,
                        NetForce = Vector2.Zero,
                        Mass = 1.0f,
                    };
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

    private void ApplyForce(int x, int y, Vector2 force)
    {
        if (!Utils.IsInGridBounds(new Vector2(x, y))) return;

        ref Particle p = ref grid[y, x];
        if (p.Type == ParticleType.Empty) return;

        p.NetForce += force;
    }

    private void IntegrateForces(float dt)
    {
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                ref Particle p = ref grid[y, x];
                if (p.Type != ParticleType.Sand && p.Type != ParticleType.Water) continue;

                Vector2 acceleration = p.NetForce / p.Mass;
                p.Velocity += acceleration * dt;

                p.NetForce = Vector2.Zero;
            }
        }
    }





}
