using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Water : Liquid
    {
        public override string DisplayName => "Water";
        public override MaterialType Type => MaterialType.Water;

        public Water(Vector2 worldPos) : base(worldPos)
        {
            Color baseColor = Color.DarkBlue;

            // 1 in 10 chance to modify brightness
            if (Raylib.GetRandomValue(0, 20) == 0)
            {
                float brightnessFactor = Raylib.GetRandomValue(70, 130) / 100f; // 0.7x to 1.3x
                int r = Math.Clamp((int)(baseColor.R * brightnessFactor), 0, 255);
                int g = Math.Clamp((int)(baseColor.G * brightnessFactor), 0, 255);
                int b = Math.Clamp((int)(baseColor.B * brightnessFactor), 0, 255);
                Color = new Color(r, g, b, 255);
            }
            else
            {
                Color = baseColor;
            }

            Mass = 1.0f;
            VerticalDamping = 0.3f;
            Health = 300;
            ExplosionResistance = 1f;
        }


        public override void Step(IMaterialContext grid)
        {
            base.Step(grid); // Preserves default Liquid/Material behavior (like movement)

            //ExtinguishNearbyFire(grid);
        }

        private void ExtinguishNearbyFire(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = x + dx;
                int ny = y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor != null && neighbor.IsOnFire)
                {
                    neighbor.IsOnFire = false;

                    // Also extinguish its neighbors
                    foreach (var (dx2, dy2) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
                    {
                        int nnx = nx + dx2;
                        int nny = ny + dy2;
                        if (!grid.IsValidCell(nnx, nny)) continue;

                        var secondNeighbor = grid.Get(nnx, nny);
                        if (secondNeighbor != null && secondNeighbor.IsOnFire)
                        {
                            secondNeighbor.IsOnFire = false;
                        }
                    }

                    // Optional: spawn smoke above original burning cell
                    if (Raylib.GetRandomValue(0, 100) < 70)
                    {
                        int smokeX = nx;
                        int smokeY = ny - 1;
                        if (grid.IsValidCell(smokeX, smokeY) && grid.IsEmpty(smokeX, smokeY))
                        {
                            grid.Set(smokeX, smokeY, new Smoke(Utils.GridToWorld(new Vector2(smokeX, smokeY)), 0.4f, 1.2f));
                        }
                    }
                }
            }
        }



    }
}
