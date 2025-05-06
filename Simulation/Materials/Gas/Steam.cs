
using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Steam : Gas
    {
        public override string DisplayName => "Steam";
        public override MaterialType Type => MaterialType.Steam;

        public Steam(Vector2 pos) : base(pos)
        {
            Color = new Color(160, 160, 160, 255); // Dark gray, not too close to background
            Mass = 0.2f;
            Lifetime = 0.1f + Raylib.GetRandomValue(0, 1000) / 1000f * 0.5f;
        }

        public Steam(Vector2 pos, float minLifetime, float maxLifetime) : base(pos)
        {
            Color = new Color(160, 160, 160, 255);
            Mass = 0.2f;
            Lifetime = minLifetime + Raylib.GetRandomValue(0, 1000) / 1000f * (maxLifetime - minLifetime);
        }
    }
}
