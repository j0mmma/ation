using System.Numerics;

using Ation.Common;

namespace Ation.ParticleSimulation;

class Cell
{
    public bool Occupied { get; set; } = false;
}

enum ParticleType
{
    Empty,
    Sand,
    Water,
    Steam,
    Solid
}

struct ParticleProperties
{
    public Raylib_cs.Color Color;
    public int GravitySpeed;
}

class ParticleSim
{
    private ParticleType[,] grid = new ParticleType[
        (int)Variables.WindowHeight / Variables.PixelSize,
        (int)Variables.WindowWidth / Variables.PixelSize
        ];

    public Dictionary<ParticleType, ParticleProperties> particleProperties = new()
    {
        [ParticleType.Empty] = new ParticleProperties
        {
            Color = new Raylib_cs.Color(0, 0, 0, 0),
            GravitySpeed = (int)Variables.BaseGravity * 0
        },
        [ParticleType.Sand] = new ParticleProperties
        {
            Color = Raylib_cs.Color.Yellow,
            GravitySpeed = (int)Variables.BaseGravity
        },
    };
    public ParticleSim() { }

    public void Init()
    {
        // fill grid with 'empty' particles
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                grid[y, x] = ParticleType.Empty;
            }
        }



    }
    public void Update(float dt)
    {
        for (int y = grid.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = grid.GetLength(1) - 1; x >= 0; x--)
            {
                if (grid[y, x] != ParticleType.Empty)
                {
                    processSand(x, y);
                }
            }
        }
    }

    private void processSand(int x, int y)
    {
        ParticleType type = grid[y, x];
        if (type == ParticleType.Empty) return;

        int gravitySpeed = particleProperties[type].GravitySpeed;

        for (int fall = 0; fall < gravitySpeed; fall++)
        {
            if (y + 1 >= grid.GetLength(0)) return;

            if (grid[y + 1, x] == ParticleType.Empty)
            {
                SwapCells(x, y, x, y + 1);
                y += 1;
            }
            else
            {
                bool canMoveLeft = (x > 0 && grid[y + 1, x - 1] == ParticleType.Empty);
                bool canMoveRight = (x < grid.GetLength(1) - 1 && grid[y + 1, x + 1] == ParticleType.Empty);

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
                    if (Raylib_cs.Raylib.GetRandomValue(0, 2) != 0) // 2/3 chance to move left
                    {
                        SwapCells(x, y, x - 1, y + 1);
                        x -= 1;
                        y += 1;
                    }
                }
                else if (canMoveRight)
                {
                    if (Raylib_cs.Raylib.GetRandomValue(0, 2) != 0) // 2/3 chance to move right
                    {
                        SwapCells(x, y, x + 1, y + 1);
                        x += 1;
                        y += 1;
                    }
                }
                else
                {
                    return; // Blocked
                }
            }
        }
    }


    private void SwapCells(int origX, int origY, int x, int y)
    {
        ParticleType temp = grid[y, x];
        grid[y, x] = grid[origY, origX];
        grid[origY, origX] = temp;
    }

    public void Render()
    {

        int gridHeight = grid.GetLength(0);
        int gridWidth = grid.GetLength(1);

        Raylib_cs.Raylib.BeginDrawing();
        Raylib_cs.Raylib.ClearBackground(Raylib_cs.Color.LightGray);




        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2 pos = Utils.GridToWorld(new Vector2(x, y));

                // Draw background grid lines
                // Raylib_cs.Raylib.DrawRectangleLines(
                //     (int)pos.X,
                //     (int)pos.Y,
                //     Variables.PixelSize,
                //     Variables.PixelSize,
                //     Raylib_cs.Color.Gray
                // );

                ParticleType type = grid[y, x];
                if (type == ParticleType.Empty) continue;

                Raylib_cs.Color color = particleProperties[type].Color;

                if (color.A > 0)
                {
                    Raylib_cs.Raylib.DrawRectangle(
                        (int)pos.X,
                        (int)pos.Y,
                        Variables.PixelSize,
                        Variables.PixelSize,
                        color
                    );
                }
            }
        }

        Raylib_cs.Raylib.DrawText($"FPS: {Raylib_cs.Raylib.GetFPS()}", 12, 12, 20, Raylib_cs.Color.Black);
        Raylib_cs.Raylib.DrawText($"# of particles: {CountParticles()}", 12, 35, 20, Raylib_cs.Color.Black);


        Raylib_cs.Raylib.EndDrawing();
    }

    public void AddParticle(Vector2 position)
    {
        Vector2 gridPos = Utils.WorldToGrid(position);

        if (Utils.IsInGridBounds(gridPos))
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            if (grid[y, x] == ParticleType.Empty)
            {
                grid[y, x] = ParticleType.Sand;
            }
        }
    }

    public int CountParticles()
    {
        int count = 0;
        foreach (var cell in grid)
        {
            if (cell != ParticleType.Empty)
                count++;
        }
        return count;
    }

}