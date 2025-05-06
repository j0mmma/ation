using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public class Stone : ImmovableSolid
    {
        public override string DisplayName => "Stone";
        public override MaterialType Type => MaterialType.Stone;

        public Stone(Vector2 pos) : base(pos)
        {
            Color = new Color(80, 80, 80, 255); // Dark gray
            Mass = float.PositiveInfinity;
            Health = 800;
            Flammability = 0f; // Completely non-flammable
            ExplosionResistance = 0.9f;
        }

        public override void Step(IMaterialContext grid)
        {
            UpdatedThisFrame = true;
            IsActive = false;
            // No burning logic needed
        }
    }
}
