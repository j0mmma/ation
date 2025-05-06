using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Wood : ImmovableSolid
    {
        public override string DisplayName => "Wood";
        public override MaterialType Type => MaterialType.Wood;

        public Wood(Vector2 pos) : base(pos)
        {
            Color = Raylib_cs.Color.Brown;
            Mass = float.PositiveInfinity;
            Health = 350;
            Flammability = 0.7f; // Highly flammable
            ExplosionResistance = 0.4f;
        }

        public override void Step(IMaterialContext grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;

            // Skip burning logic if extinguished


            BurnIfIgnited(grid);
        }


        private void BurnIfIgnited(IMaterialContext grid)
        {
            if (ExtinguishIfSubmerged(grid))
                return;

            if (!IsOnFire || !Health.HasValue)
                return;


            float dt = Raylib.GetFrameTime();

            float effectiveBurnRate = 25f;
            foreach (var (dx, dy) in new[] { (0, 5), (0, -5), (5, 0), (-5, 0) })
            {
                int nx = (int)gridPos.X + dx;
                int ny = (int)gridPos.Y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor is Fire fire)
                {
                    effectiveBurnRate = fire.Damage;
                    break;
                }
            }

            Health -= effectiveBurnRate * dt;

            UpdateBurnedAppearance();

            // Emit smoke
            if (Raylib.GetRandomValue(0, 100) < 10)
                TrySpawnSmoke(grid);

            // Spawn fire above
            if (Raylib.GetRandomValue(0, 100) < 70)
                TrySpawnFire(grid);

            // Spread fire only if low health
            if (Health < Health * 0.75)
            {
                if (Random.Shared.NextDouble() < Flammability)
                    TrySpreadFireToNeighbor(grid);
            }

            // Destroy when very low health
            if (Health < Health * 0.35)
            {
                if (Raylib.GetRandomValue(0, 100) < 10)
                {
                    SpreadFireAggressively(grid);
                    grid.Set((int)gridPos.X, (int)gridPos.Y, new Smoke(Utils.GridToWorld(gridPos), 0.6f, 2f));
                }
            }
            else if (Health <= 0)
            {
                grid.Set((int)gridPos.X, (int)gridPos.Y, new Smoke(Utils.GridToWorld(gridPos), 0.6f, 2f));
            }


        }

        private void TrySpreadFireToNeighbor(IMaterialContext grid)
        {
            var directions = new (int dx, int dy)[]
            {
                (0, 1), (0, -1), (1, 0), (-1, 0)
            }.OrderBy(_ => Raylib.GetRandomValue(0, 100)).ToArray();

            foreach (var (dx, dy) in directions)
            {
                int nx = (int)gridPos.X + dx;
                int ny = (int)gridPos.Y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor is Wood neighborWood && !neighborWood.IsOnFire)
                {
                    if ((float)Random.Shared.NextDouble() < neighborWood.Flammability)
                    {
                        neighborWood.IsOnFire = true;
                        break;
                    }
                }
            }
        }

        private void SpreadFireAggressively(IMaterialContext grid)
        {
            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = (int)gridPos.X + dx;
                int ny = (int)gridPos.Y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor is Wood neighborWood && !neighborWood.IsOnFire)
                {
                    neighborWood.IsOnFire = true;
                }
            }
        }

        private void TrySpawnSmoke(IMaterialContext grid)
        {
            int smokeX = (int)gridPos.X;
            int smokeY = (int)gridPos.Y - 1;
            if (grid.IsValidCell(smokeX, smokeY) && grid.IsEmpty(smokeX, smokeY))
                grid.Set(smokeX, smokeY, new Smoke(Utils.GridToWorld(new Vector2(smokeX, smokeY)), 0.5f, 1.5f));
        }

        private void TrySpawnFire(IMaterialContext grid)
        {
            int fireX = (int)gridPos.X;
            int fireY = (int)gridPos.Y - 1;
            if (grid.IsValidCell(fireX, fireY) && grid.IsEmpty(fireX, fireY))
                grid.Set(fireX, fireY, new Fire(Utils.GridToWorld(new Vector2(fireX, fireY))));
        }

        private bool ExtinguishIfSubmerged(IMaterialContext grid)
        {
            bool extinguished = false;

            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nx = (int)gridPos.X + dx;
                int ny = (int)gridPos.Y + dy;
                if (!grid.IsValidCell(nx, ny)) continue;

                var neighbor = grid.Get(nx, ny);
                if (neighbor?.Type == MaterialType.Water)
                {
                    IsOnFire = false;
                    extinguished = true;

                    // Also extinguish directly adjacent neighbors
                    foreach (var (dx2, dy2) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
                    {
                        int nnx = (int)gridPos.X + dx2;
                        int nny = (int)gridPos.Y + dy2;
                        if (!grid.IsValidCell(nnx, nny)) continue;

                        var adjacent = grid.Get(nnx, nny);
                        if (adjacent is Material m && m.IsOnFire)
                            m.IsOnFire = false;
                    }

                    if (Raylib.GetRandomValue(0, 100) < 60)
                        //grid.Set((int)gridPos.X, (int)gridPos.Y, new Smoke(Utils.GridToWorld(gridPos), 0.4f, 1.2f));

                        break;
                }
            }

            return extinguished;
        }





        public void UpdateBurnedAppearance()
        {
            if (!Health.HasValue) return;

            float healthPercent = MathF.Max(0, Health.Value / 500);
            byte r = (byte)(139 * healthPercent);
            byte g = (byte)(69 * healthPercent);
            byte b = (byte)(19 * healthPercent);

            Color = new Color(r, g, b, (byte)255);
        }
    }
}
