using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Particle : Material
    {
        private Vector2 velocity;
        private float lifetime = 2f;
        private readonly Material? carriedMaterial;

        public override string DisplayName => "Particle";
        public override MaterialType Type => MaterialType.Particle;

        public Particle(Vector2 worldPos, Vector2 initialVelocity, Color color, Material? carriedMaterial = null)
            : base(worldPos)
        {
            this.velocity = initialVelocity;
            this.Color = color;
            this.carriedMaterial = carriedMaterial;
        }

        public override void Step(IMaterialContext grid)
        {
            float dt = Raylib.GetFrameTime();

            velocity += FallingSandSim.Gravity * dt;
            velocity.X += Raylib.GetRandomValue(-100, 100) / 1000f * 50f;
            velocity = Vector2.Clamp(velocity, new Vector2(-800, -800), new Vector2(800, 800));

            lifetime -= dt;

            Vector2 newWorldPos = worldPos + velocity * dt;
            Vector2 newGridPos = Utils.WorldToGrid(newWorldPos);
            int nx = (int)newGridPos.X;
            int ny = (int)newGridPos.Y;

            if (!grid.IsValidCell(nx, ny))
            {
                TryReplaceSelf(grid);
                return;
            }

            var target = grid.Get(nx, ny);

            if (target == null || target is Gas || target is Particle)
            {
                grid.Swap((int)gridPos.X, (int)gridPos.Y, nx, ny);
                gridPos = newGridPos;
                worldPos = newWorldPos;
            }
            else
            {
                TryReplaceSelf(grid);
            }
        }

        private void TryReplaceSelf(IMaterialContext grid)
        {
            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            if (carriedMaterial != null)
            {
                Material toPlace = carriedMaterial is ImmovableSolid
                    ? new FallingImmovable(carriedMaterial)
                    : carriedMaterial;

                grid.Set(x, y, toPlace);
            }
            else
            {
                grid.Clear(x, y);
            }
        }
    }

}
