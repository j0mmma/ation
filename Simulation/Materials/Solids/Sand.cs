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
            Health = 150;
            ExplosionResistance = 0.5f;
        }

        protected override bool ActOnImmovableSolid(ImmovableSolid solid)
        {
            // Instead of turning red (like default MovableSolid), turn GREEN
            SetActive(); // Mark as changed so it updates visually

            return false; // No swap happens, just color change
        }
    }
}
