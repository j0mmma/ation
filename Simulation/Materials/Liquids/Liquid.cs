using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class Liquid : Material
    {
        protected float speedClamp = 200f;
        protected float friction = 0.1f;
        public float VerticalDamping { get; protected set; } = 0.5f;
        public float TurbulenceStrength { get; protected set; } = 0.01f;
        protected virtual int MaxHorizontalDispersion => 10;


        public Liquid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(IMaterialContext grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;

            float dt = Raylib.GetFrameTime();
            ApplyForce(FallingSandSim.Gravity);
            Integrate(dt);

            movementRemainder += Velocity * dt;
            int steps = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.Y -= steps;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;
            bool preferLeft = Raylib.GetRandomValue(0, 1) == 0;
            int maxDispersion = MaxHorizontalDispersion;

            for (int i = 0; i < Math.Abs(steps); i++)
            {
                if (TryMove(grid, ref x, ref y, 0, 1))
                    continue;

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

                Velocity *= friction;
                break;
            }

            Velocity = Vector2.Clamp(
                Velocity,
                new Vector2(-speedClamp, -speedClamp),
                new Vector2(speedClamp, speedClamp)
            );
            worldPos = Utils.GridToWorld(gridPos);
        }

        private bool TryMove(IMaterialContext grid, ref int x, ref int y, int dx, int dy)
        {
            int nx = x + dx, ny = y + dy;
            if (!grid.IsValidCell(nx, ny)) return false;

            var target = grid.Get(nx, ny);
            if (!CanDisplace(target)) return false;

            if (dy == 0)
            {
                var below = grid.Get(nx, ny + 1);
                if (below == null) return false;

                var cornerBlocker = grid.Get(x + dx, y + 1);
                if (cornerBlocker is ImmovableSolid)
                    return false;
            }

            grid.Swap(x, y, nx, ny);
            x = nx;
            y = ny;
            gridPos = new Vector2(x, y);
            IsActive = true;
            return true;
        }

        protected virtual bool CanDisplace(Material target)
        {
            if (target == null)
                return true;

            if (target is Liquid otherLiquid)
                return this.Mass > otherLiquid.Mass;

            return false;
        }
    }
}
