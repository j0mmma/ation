using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    public class FallingImmovable : MovableSolid
    {
        public override bool IsCollidable => true;

        public override string DisplayName => "Debris";
        public override MaterialType Type => MaterialType.FallingImmovable;

        public FallingImmovable(Material source)
            : base(source.worldPos)
        {
            this.Color = source.Color;
            this.Health = source.Health;
            this.Flammability = source.Flammability;
            this.Mass = 2f; // Lighter than original, falls
        }

        public override void Step(IMaterialContext grid)
        {
            base.Step(grid);
            // Optional: Add extra behavior like crumbling, etc.
        }
    }

}
