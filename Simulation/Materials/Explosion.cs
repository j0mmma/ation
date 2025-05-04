using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Explosion
    {
        private readonly IMaterialContext grid;
        private readonly int centerX;
        private readonly int centerY;
        private readonly int radius;
        private readonly float force;

        public Explosion(IMaterialContext grid, int centerX, int centerY, int radius, float force)
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

                    Vector2 dir = Vector2.Normalize(new Vector2(dx, dy - 2));
                    Vector2 impulse = dir * (force * falloff) + new Vector2(0, -force * 1.2f);

                    var current = grid.Get(x, y);

                    // Launch real material if present
                    if (current != null && !(current is Gas) && !(current is Particle))
                    {

                        var flying = new Particle(
                            Utils.GridToWorld(new Vector2(x, y)),
                            impulse,
                            current.Color,
                            current
                        );
                        grid.Set(x, y, flying);
                    }
                    // Otherwise maybe spawn smoke
                    if (Raylib.GetRandomValue(0, 100) < 40)
                    {
                        var smoke = new Smoke(Utils.GridToWorld(new Vector2(x, y)));
                        grid.Set(x, y, smoke);
                    }
                }
            }
        }

    }
}
