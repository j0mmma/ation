using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Sand : MovableSolid
    {
        public override string DisplayName => "Sand";
        public override MaterialType Type => MaterialType.Sand;

        public Sand(Vector2 pos) : base(pos)
        {
            Color = Color.Yellow;
            Mass = 1.0f;
        }
    }
}
