using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Water : Liquid
    {
        public override string DisplayName => "Water";
        public override MaterialType Type => MaterialType.Sand;

        public Water(Vector2 worldPos) : base(worldPos)
        {
            Color = Color.Blue;
            Mass = 1.0f;
        }
    }
}
