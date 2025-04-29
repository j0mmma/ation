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
            Color = Color.Pink;
        }

        public override void Step(SimulationGrid grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            // Erase neighbors immediately (3x3)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (grid.IsValidCell(nx, ny))
                    {
                        grid.Clear(nx, ny); // Immediate erase
                    }
                }
            }

            // Erase self
            grid.Clear(x, y);
        }
    }
}
