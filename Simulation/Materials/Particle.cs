using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Particle : Material
    {
        private Vector2 velocity;
        private float lifetime = 2f; // shorter lifetime for visual tests

        public override string DisplayName => "Particle";
        public override MaterialType Type => MaterialType.Particle; // no special type

        public Particle(Vector2 worldPos, Vector2 initialVelocity, Color color)
            : base(worldPos)
        {
            this.velocity = initialVelocity;
            this.Color = color;
        }

        public override void Step(SimulationGrid grid)
        {
            float dt = Raylib.GetFrameTime();

            // Apply gravity and optional small turbulence
            velocity += FallingSandSim.Gravity * dt;
            velocity.X += Raylib.GetRandomValue(-100, 100) / 1000f * 50f; // optional horizontal wobble
            velocity = Vector2.Clamp(velocity, new Vector2(-800, -800), new Vector2(800, 800));

            // Decrease lifetime
            lifetime -= dt;
            if (lifetime <= 0)
            {
                grid.Clear((int)gridPos.X, (int)gridPos.Y);
                return;
            }

            // Move
            Vector2 newWorldPos = worldPos + velocity * dt;
            Vector2 newGridPos = Utils.WorldToGrid(newWorldPos);
            int nx = (int)newGridPos.X;
            int ny = (int)newGridPos.Y;

            if (!grid.IsValidCell(nx, ny))
            {
                grid.Clear((int)gridPos.X, (int)gridPos.Y);
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
                // If it hits something solid/liquid -> just clear itself (disappear)
                grid.Clear((int)gridPos.X, (int)gridPos.Y);
            }
        }
    }
}
