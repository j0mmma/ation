using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class Liquid : Material
    {
        protected float speedClamp = 200f;
        protected float friction = 0.9f;
        // flip once per frame to avoid bias
        private static bool toggleFlowDirection = false;

        public Liquid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(SimulationGrid grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;

            // 1) apply gravity & integrate
            float dt = Raylib.GetFrameTime();
            ApplyForce(FallingSandSim.Gravity);
            Integrate(dt);

            // 2) accumulate Y movement
            movementRemainder += Velocity * dt;
            int steps = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.Y -= steps;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;
            bool preferLeft = Raylib.GetRandomValue(0, 1) == 0;
            const int maxDispersion = 8;

            // 3) try to move down, diag, then extended horizontal
            for (int i = 0; i < Math.Abs(steps); i++)
            {
                // straight down
                if (TryMove(grid, ref x, ref y, 0, 1))
                    continue;

                // diagonal (biased)
                if (preferLeft)
                {
                    if (TryMove(grid, ref x, ref y, -1, 1)) continue;
                    if (TryMove(grid, ref x, ref y, +1, 1)) continue;
                }
                else
                {
                    if (TryMove(grid, ref x, ref y, +1, 1)) continue;
                    if (TryMove(grid, ref x, ref y, -1, 1)) continue;
                }

                // extended horizontal scan
                bool movedHorizontally = false;
                for (int d = 1; d <= maxDispersion; d++)
                {
                    if (preferLeft)
                    {
                        if (TryMove(grid, ref x, ref y, -d, 0)) { movedHorizontally = true; break; }
                        if (TryMove(grid, ref x, ref y, +d, 0)) { movedHorizontally = true; break; }
                    }
                    else
                    {
                        if (TryMove(grid, ref x, ref y, +d, 0)) { movedHorizontally = true; break; }
                        if (TryMove(grid, ref x, ref y, -d, 0)) { movedHorizontally = true; break; }
                    }
                }
                if (movedHorizontally)
                    continue;

                // if totally blocked, apply friction and stop
                Velocity *= friction;
                break;
            }

            // 4) clamp & sync world pos
            Velocity = Vector2.Clamp(
                Velocity,
                new Vector2(-speedClamp, -speedClamp),
                new Vector2(speedClamp, speedClamp)
            );
            worldPos = Utils.GridToWorld(gridPos);
        }

        // helper to attempt a swap at (dx,dy)
        private bool TryMove(SimulationGrid grid, ref int x, ref int y, int dx, int dy)
        {
            int nx = x + dx, ny = y + dy;
            if (!grid.IsValidCell(nx, ny) || !grid.IsEmpty(nx, ny))
                return false;

            grid.Swap(x, y, nx, ny);
            x = nx;
            y = ny;
            gridPos = new Vector2(x, y);
            IsActive = true;
            return true;
        }


    }
}