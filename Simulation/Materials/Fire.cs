using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Fire : Material
    {
        protected float speedClamp = 90f;
        protected float friction = 0.85f;

        public override string DisplayName => "Fire";
        public override MaterialType Type => MaterialType.Fire;

        public Fire(Vector2 pos) : base(pos)
        {
            Color = Color.Orange;
            Mass = 0.1f;
            Lifetime = 0.03f + Raylib.GetRandomValue(0, 1000) / 1000f * 0.05f;
            Damage = 300f;
        }

        public override void Step(IMaterialContext grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;

            float dt = Raylib.GetFrameTime();

            if (CheckExtinguishedByWater(grid))
                return;

            // Lifetime countdown
            if (Lifetime.HasValue)
            {
                Lifetime -= dt;
                if (Lifetime <= 0)
                {
                    grid.Clear((int)gridPos.X, (int)gridPos.Y);
                    return;
                }
            }

            MarkFlammableNeighbors(grid);

            // Movement
            ApplyForce(FallingSandSim.Gravity * new Vector2(0, -0.1f));
            Integrate(dt);

            movementRemainder += Velocity * dt;
            int steps = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.Y -= steps;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;
            bool preferLeft = Raylib.GetRandomValue(0, 1) == 0;
            const int maxDispersion = 10;

            for (int i = 0; i < Math.Abs(steps); i++)
            {
                if (TryMove(grid, ref x, ref y, 0, -1)) continue;
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

                for (int d = 1; d <= maxDispersion; d++)
                {
                    if (preferLeft)
                    {
                        if (TryMove(grid, ref x, ref y, -d, 0)) break;
                        if (TryMove(grid, ref x, ref y, +d, 0)) break;
                    }
                    else
                    {
                        if (TryMove(grid, ref x, ref y, +d, 0)) break;
                        if (TryMove(grid, ref x, ref y, -d, 0)) break;
                    }
                }

                Velocity *= friction;
                break;
            }

            Velocity = Vector2.Clamp(Velocity, new Vector2(-speedClamp, -speedClamp), new Vector2(speedClamp, speedClamp));
            worldPos = Utils.GridToWorld(gridPos);
        }

        private void MarkFlammableNeighbors(IMaterialContext grid)
        {
            if (Lifetime > 0.01f) return; // Fire is still burning strong — don't spread yet

            int cx = (int)gridPos.X;
            int cy = (int)gridPos.Y;

            var directions = new (int dx, int dy)[]
            {
        (0, 1),  // Down
        (0, -1), // Up
        (-1, 0), // Left
        (1, 0)   // Right
            };

            // Shuffle directions to randomize spread
            directions = directions.OrderBy(_ => Raylib.GetRandomValue(0, 100)).ToArray();

            foreach (var (dx, dy) in directions)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor != null && neighbor is ImmovableSolid && !neighbor.IsOnFire)
                {
                    neighbor.IsOnFire = true;
                    break; // only ignite one per step
                }
            }
        }
        private bool CheckExtinguishedByWater(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            var cell = grid.Get(x, y);
            if (cell != null && cell.Type == MaterialType.Water)
            {
                grid.Set(x, y, new Smoke(Utils.GridToWorld(gridPos), 0.3f, 1f));
                return true;
            }

            return false;
        }



        private bool TryMove(IMaterialContext grid, ref int x, ref int y, int dx, int dy)
        {
            int nx = x + dx, ny = y + dy;
            if (!grid.IsValidCell(nx, ny) || !grid.IsEmpty(nx, ny)) return false;

            grid.Swap(x, y, nx, ny);
            x = nx;
            y = ny;
            gridPos = new Vector2(x, y);
            IsActive = true;
            return true;
        }
    }
}
