using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Acid : Liquid
    {
        public override string DisplayName => "Acid";
        public override MaterialType Type => MaterialType.Acid;

        private static readonly Random rng = new();

        public Acid(Vector2 worldPos) : base(worldPos)
        {
            Color = new Color(0, 255, 0, 255);
            Mass = 1.0f;
            Damage = 50f;
            Health = 1000;
        }

        public override void Step(SimulationGrid grid)
        {


            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            float dt = Raylib.GetFrameTime();
            float totalDamageThisFrame = 0f;

            foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                var neighbor = grid.Get(x + dx, y + dy);
                if (neighbor == null) continue;

                bool damaged = neighbor switch
                {
                    MovableSolid m => ActOnMovableSolid(m, x + dx, y + dy, grid),
                    ImmovableSolid i => ActOnImmovableSolid(i, x + dx, y + dy, grid),
                    Liquid l => ActOnLiquid(l, x + dx, y + dy, grid),
                    _ => false
                };

                if (damaged)
                {
                    totalDamageThisFrame += Damage * dt;
                    grid.Set(x, y, new AcidVapor(Utils.GridToWorld(gridPos)));
                }
            }

            if (Health.HasValue)
                Health -= totalDamageThisFrame;

            if (Health is <= 0)
                grid.Set(x, y, new AcidVapor(Utils.GridToWorld(gridPos)));

            base.Step(grid);
        }

        public bool ActOnMovableSolid(MovableSolid solid, int x, int y, SimulationGrid grid)
        {
            return Corrode(solid, x, y, grid);
        }

        protected bool ActOnImmovableSolid(ImmovableSolid solid, int x, int y, SimulationGrid grid)
        {
            return Corrode(solid, x, y, grid);
        }

        protected bool ActOnLiquid(Liquid liquid, int x, int y, SimulationGrid grid)
        {
            return Corrode(liquid, x, y, grid);
        }

        private bool Corrode(Material target, int x, int y, SimulationGrid grid)
        {
            if (target.Type is MaterialType.Wood or MaterialType.Sand or MaterialType.Water)
            {
                if (rng.NextDouble() < 0.05)
                {
                    grid.Clear(x, y);
                    return true;
                }
            }

            return false;
        }
    }
}
