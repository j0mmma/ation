using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    class FallingSandSim
    {
        private readonly SimulationGrid grid;
        public static Vector2 Gravity = new Vector2(0, 2000f);
        private int frameCounter = 0;

        public FallingSandSim(int width, int height)
        {
            grid = new SimulationGrid(width, height);
        }

        public void Update(float dt)
        {
            grid.ResetFlags(); // Reset UpdatedThisFrame = false for all

            frameCounter++;
            bool flipX = frameCounter % 2 == 0; // Flip X scan every frame

            for (int y = grid.Size.Height - 1; y >= 0; y--)
            {
                if (flipX)
                {
                    for (int x = grid.Size.Width - 1; x >= 0; x--)
                    {
                        var m = grid.Get(x, y);
                        if (m == null || m.UpdatedThisFrame) continue;

                        m.Step(grid);
                    }
                }
                else
                {
                    for (int x = 0; x < grid.Size.Width; x++)
                    {
                        var m = grid.Get(x, y);
                        if (m == null || m.UpdatedThisFrame) continue;

                        m.Step(grid);
                    }
                }
            }
        }

        public void Render()
        {
            for (int y = 0; y < grid.Size.Height; y++)
            {
                for (int x = 0; x < grid.Size.Width; x++)
                {
                    var m = grid.Get(x, y);
                    if (m == null) continue;

                    var pos = Utils.GridToWorld(new Vector2(x, y));
                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, Variables.PixelSize, Variables.PixelSize, m.Color);
                }
            }
        }

        public void Explode(int cx, int cy, int radius, float force)
        {
            var explosion = new Explosion(grid, cx, cy, radius, force);
            explosion.Enact();
        }

        public void AddMaterial(Vector2 worldPos, MaterialType type, int radius = 3)
        {
            var gridPos = Utils.WorldToGrid(worldPos);
            int cx = (int)gridPos.X;
            int cy = (int)gridPos.Y;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int gx = cx + x;
                    int gy = cy + y;
                    if (x * x + y * y > radius * radius) continue;
                    if (!grid.IsValidCell(gx, gy)) continue;
                    if (grid.IsEmpty(gx, gy))
                    {
                        var world = Utils.GridToWorld(new Vector2(gx, gy));
                        grid.Set(gx, gy, MaterialFactory.Create(type, world));
                    }
                }
            }
        }

        public void ClearMaterials(Vector2 worldPos, int radius = 3)
        {
            var gridPos = Utils.WorldToGrid(worldPos);
            int cx = (int)gridPos.X;
            int cy = (int)gridPos.Y;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int gx = cx + x;
                    int gy = cy + y;
                    if (x * x + y * y > radius * radius) continue;
                    if (grid.IsValidCell(gx, gy))
                        grid.Clear(gx, gy);
                }
            }
        }

        public int CountMaterials() => grid.Count();
    }

    // TODO: implement chunks here
}
