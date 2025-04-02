using System.Numerics;
using Raylib_cs;

namespace Ation.Systems;

class Cell
{
    public bool Occupied { get; set; } = false;
}

class ParticleSim
{
    public Cell[,] grid = new Cell[500, 500];
    public ParticleSim() { }

    public void Update(float dt)
    {
        for (int i = grid.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = grid.GetLength(1) - 1; j >= 0; j--)
            {
                if (grid[i, j] != null)
                {
                    processSand(i, j);
                }
            }
        }
    }
    private void processSand(int i, int j)
    {
        // TODO Implement
    }
    private bool PositionInScreenBounds(Vector2 pos)
    {
        if (pos.X > 0 || pos.X < 500 || pos.Y > 0 || pos.Y < 500)
            return true;
        return false;
    }

    private void SwapCells(int origX, int origY, int x, int y)
    {
        //     if (origX < 0 || origX >= gridWidth || origY < 0 || origY >= gridHeight ||
        //    x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        //     {
        //         Console.WriteLine("Invalid coordinates");
        //         return;
        //     }

        Cell temp = grid[x, y];
        grid[x, y] = grid[origX, origY];
        grid[origX, origY] = temp;
    }
    public void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.LightGray);



        // draw grid

        const int screenWidth = 800;
        const int screenHeight = 800;

        const int gridWidth = 200;   // Number of columns
        const int gridHeight = 200;  // Number of rows
        const int pixelSize = 4;   // Size of each "pixel" (square)

        // Draw the grid of pixels
        for (int y = 0; y < gridHeight; y++)  // Rows
        {
            for (int x = 0; x < gridWidth; x++)  // Columns
            {
                int posX = x * pixelSize; // X-coordinate of the pixel
                int posY = y * pixelSize; // Y-coordinate of the pixel

                // Example: Alternate colors for the grid
                // Raylib.DrawText($"# of entities: {entityManager.GetSize()}", 12, 25, 20, Color.Black);

                Raylib.DrawRectangleLines(posX, posY, pixelSize, pixelSize, Color.Gray);

                // Raylib.DrawRectangle(posX, posY, pixelSize, pixelSize, color);
            }
        }

        Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
        Raylib.DrawText($"# of particles: {this.CountParticles()}", 12, 25, 20, Color.Black);


        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                var pos = new Vector2(i * 4, j * 4);
                if (PositionInScreenBounds(pos) && grid[i, j] != null)
                {
                    Raylib.DrawRectangle(
                    (int)pos.X,
                    (int)pos.Y,
                    4,
                    4,
                    Raylib_cs.Color.Red
                 );
                }
            }
        }

        // 

        Raylib.EndDrawing();

    }
    public void AddParticle(Vector2 position)
    {
        // FIX: dont add particles if mouse dragged outside the window when button down
        // grid coordinates
        int x = (int)position.X / 4;
        int y = (int)position.Y / 4;
        if (PositionInScreenBounds(position) && grid[x, y] == null)
            grid[x, y] = new Cell();
    }

    public int CountParticles()
    {
        int count = 0;
        foreach (var cell in grid)
        {
            if (cell != null)
                count++;
        }
        return count;
    }
}