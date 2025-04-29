using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Explosion
    {
        private readonly SimulationGrid grid;
        private readonly int centerX;
        private readonly int centerY;
        private readonly int radius;
        private readonly float force;

        public Explosion(SimulationGrid grid, int centerX, int centerY, int radius, float force)
        {
            this.grid = grid;
            this.centerX = centerX;
            this.centerY = centerY;
            this.radius = radius;
            this.force = force;
        }

        public void Enact()
        {
            int sqrRadius = radius * radius;

            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (!grid.IsValidCell(x, y)) continue;

                    int dx = x - centerX;
                    int dy = y - centerY;
                    int distSq = dx * dx + dy * dy;

                    if (distSq > sqrRadius)
                        continue;

                    float dist = MathF.Sqrt(distSq);
                    float falloff = 1f - (dist / radius);

                    // // Spawn new flying particles ONLY
                    // Vector2 dir = Vector2.Normalize(new Vector2(dx, dy)); // upward bias
                    // Vector2 impulse = dir * (force * falloff) + new Vector2(0, -force * 1.7f);

                    Vector2 dir = Vector2.Normalize(new Vector2(dx, dy));
                    Vector2 impulse = dir * force;

                    var flying = new Particle(
                        Utils.GridToWorld(new Vector2(x, y)),
                        impulse,
                        Raylib_cs.Color.Red
                        );

                    if (Raylib.GetRandomValue(0, 100) > 50) continue;
                    grid.Set(x, y, flying);
                }
            }
        }
    }
}
