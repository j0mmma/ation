using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class Gas : Material
    {
        protected float speedClamp = 100f;
        protected float friction = 0.95f;

        public Gas(Vector2 worldPos) : base(worldPos) { }

        public override void Step(SimulationGrid grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;
            float dt = Raylib.GetFrameTime();

            // Lifetime handling
            if (Lifetime.HasValue)
            {
                Lifetime -= dt;
                if (Lifetime <= 0)
                {
                    grid.Clear((int)gridPos.X, (int)gridPos.Y);
                    return;
                }
            }


            // 1) apply upward force (buoyancy) & integrate
            ApplyForce(FallingSandSim.Gravity * new Vector2(0, -0.2f));
            Integrate(dt);

            // 2) accumulate Y movement
            movementRemainder += Velocity * dt;
            int steps = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.Y -= steps;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;
            bool preferLeft = Raylib.GetRandomValue(0, 1) == 0;
            const int maxDispersion = 25;

            // 3) try to move up, diag, then extended horizontal
            for (int i = 0; i < Math.Abs(steps); i++)
            {
                // straight up
                if (TryMove(grid, ref x, ref y, 0, -1))
                    continue;

                // diagonal (biased)
                if (preferLeft)
                {
                    if (TryMove(grid, ref x, ref y, -1, -1)) continue;
                    if (TryMove(grid, ref x, ref y, +1, -1)) continue;
                }
                else
                {
                    if (TryMove(grid, ref x, ref y, +1, -1)) continue;
                    if (TryMove(grid, ref x, ref y, -1, -1)) continue;
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
            if (!grid.IsValidCell(nx, ny))
                return false;

            var target = grid.Get(nx, ny);
            if (target != null && target.Type != MaterialType.Water)
                return false;

            grid.Swap(x, y, nx, ny);
            x = nx;
            y = ny;
            gridPos = new Vector2(x, y);
            IsActive = true;
            return true;
        }

        // public override bool ActOnNeighbor(Material neighbor, int targetX, int targetY, SimulationGrid grid)
        // {
        //     // Gas typically doesn't push anything
        //     return false;
        // }
    }
}
