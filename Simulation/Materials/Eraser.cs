using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Eraser : Material
    {
        public override MaterialType Type => MaterialType.Eraser;
        public override string DisplayName => "Eraser";

        public Eraser(Vector2 worldPos) : base(worldPos)
        {
            Color = Color.Pink; // or whatever color you want for eraser
        }

        public override void Step(SimulationGrid grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            // Erase neighbors (3x3 area around)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (grid.IsValidCell(nx, ny))
                    {
                        grid.Set(nx, ny, null); // Clear the neighbor
                    }
                }
            }

            // Then erase itself
            grid.Set(x, y, null);
        }

        // public override bool ActOnNeighbor(Material neighbor, int targetX, int targetY, SimulationGrid grid)
        // {
        //     // Not used, Eraser acts in Step() directly
        //     return false;
        // }
    }
}
