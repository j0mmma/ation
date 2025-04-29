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
            Mass = 0.5f;
        }
    }
}
