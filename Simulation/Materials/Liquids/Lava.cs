using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Lava : Liquid
    {
        public override string DisplayName => "Lava";
        public override MaterialType Type => MaterialType.Lava;
        protected override int MaxHorizontalDispersion => 2;

        public Lava(Vector2 worldPos) : base(worldPos)
        {
            Color = new Color(200, 60, 10, 255);
            Mass = 3.0f;
            VerticalDamping = 0.2f;
            TurbulenceStrength = 0f;
            Health = 1000;
            ExplosionResistance = 1.0f;
            Lifetime = 5.0f + Raylib.GetRandomValue(0, 1000) / 1000f * 3.0f;
        }

        public override void Step(IMaterialContext grid)
        {
            float dt = Raylib.GetFrameTime();

            if (Lifetime.HasValue)
            {
                bool nearStone = IsNextToStone(grid);
                float coolingRate = 1f;

                if (IsSubmerged(grid))
                    coolingRate = 3.0f;
                else if (nearStone)
                    coolingRate = 1f;

                Lifetime -= dt * coolingRate;

                if (Lifetime <= 0)
                {
                    grid.Set((int)gridPos.X, (int)gridPos.Y, new Stone(Utils.GridToWorld(gridPos)));

                    if (coolingRate >= 1.0f && Raylib.GetRandomValue(0, 100) < 90)
                    {
                        int steamX = (int)gridPos.X;
                        int steamY = (int)gridPos.Y - 1;
                        if (grid.IsValidCell(steamX, steamY) && grid.IsEmpty(steamX, steamY))
                        {
                            grid.Set(steamX, steamY, new Steam(Utils.GridToWorld(new Vector2(steamX, steamY)), 0.3f, 1.0f));
                        }
                    }

                    return;
                }
            }
            TryIgniteNearbyWood(grid);

            base.Step(grid);
        }

        private bool IsSubmerged(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = x + dx;
                int ny = y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor?.Type == MaterialType.Water)
                    return true;
            }

            return false;
        }

        private bool IsNextToStone(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = x + dx;
                int ny = y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor?.Type == MaterialType.Stone)
                    return true;
            }

            return false;
        }

        private void TryIgniteNearbyWood(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = x + dx;
                int ny = y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor is Wood wood && !wood.IsOnFire)
                {
                    if (Raylib.GetRandomValue(0, 100) < 40) // 40% chance per frame
                        wood.IsOnFire = true;
                }
            }
        }

    }
}
