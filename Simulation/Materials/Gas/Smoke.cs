using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Smoke : Gas
    {
        public override string DisplayName => "Smoke";
        public override MaterialType Type => MaterialType.Smoke; // Or MaterialType.Smoke if you want a new enum

        public Smoke(Vector2 pos) : base(pos)
        {
            Color = Raylib_cs.Color.Black;
            Mass = 0.2f; // Light mass
            Lifetime = 0.1f + Raylib.GetRandomValue(0, 1000) / 1000f * 0.5f;
        }

        public Smoke(Vector2 pos, float minLifetime, float maxLifetime) : base(pos)
        {
            Color = Raylib_cs.Color.Black;
            Mass = 0.2f;
            Lifetime = minLifetime + Raylib.GetRandomValue(0, 1000) / 1000f * (maxLifetime - minLifetime);
        }
    }
}
