using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class AcidVapor : Gas
    {
        public override string DisplayName => "Acid Vapor";
        public override MaterialType Type => MaterialType.AcidVapor;

        public AcidVapor(Vector2 worldPos) : base(worldPos)
        {
            Color = new Color(150, 255, 150, 200); // Pale green, semi-transparent
            Mass = 0.2f;
            Lifetime = 0.1f + Raylib.GetRandomValue(0, 1000) / 1000f * 0.5f;
        }
    }
}
