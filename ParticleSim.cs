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
}

class ParticleSim
{
    private ParticleType[,] grid = new ParticleType[
        (int)Variables.WindowHeight / Variables.PixelSize,
        (int)Variables.WindowWidth / Variables.PixelSize
        ];

    public Dictionary<ParticleType, ParticleProperties> particleProperties = new()
    {
        [ParticleType.Empty] = new ParticleProperties { Color = new Raylib_cs.Color(0, 0, 0, 0) },
        [ParticleType.Sand] = new ParticleProperties { Color = Raylib_cs.Color.Yellow },
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

    private void processSand(int i, int j)
    {
        // TODO Implement
    }
    private void SwapCells(int origX, int origY, int x, int y)
    {
        //     if (origX < 0 || origX >= gridWidth || origY < 0 || origY >= gridHeight ||
        //    x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        //     {
        //         Console.WriteLine("Invalid coordinates");
        //         return;
        //     }

        //Cell temp = grid[x, y];
        //grid[x, y] = grid[origX, origY];
        //grid[origX, origY] = temp;
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
                Raylib_cs.Raylib.DrawRectangleLines(
                    (int)pos.X,
                    (int)pos.Y,
                    Variables.PixelSize,
                    Variables.PixelSize,
                    Raylib_cs.Color.Gray
                );

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