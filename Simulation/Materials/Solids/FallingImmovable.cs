using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    public class FallingImmovable : MovableSolid
    {
        public override bool IsCollidable => false;

        public override string DisplayName => "Debris";
        public override MaterialType Type => MaterialType.FallingImmovable;

        public FallingImmovable(Material source)
    : base(source.worldPos)
        {
            this.Color = new Color(
                (byte)(source.Color.R * 0.6f),
                (byte)(source.Color.G * 0.6f),
                (byte)(source.Color.B * 0.6f),
                source.Color.A
            );

            this.Health = source.Health;
            this.Flammability = source.Flammability;
            this.Mass = 2f;
        }


        public override void Step(IMaterialContext grid)
        {
            base.Step(grid);
            // Optional: Add extra behavior like crumbling, etc.
        }
    }

}
