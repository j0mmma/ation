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
        }
    }
}
