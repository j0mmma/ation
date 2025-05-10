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
            Color baseColor = Color.Yellow;

            // 1 in 10 chance to modify brightness
            if (Raylib.GetRandomValue(0, 9) == 0)
            {
                float brightnessFactor = Raylib.GetRandomValue(50, 120) / 100f; // 0.8x to 1.2x
                int r = Math.Clamp((int)(baseColor.R * brightnessFactor), 0, 255);
                int g = Math.Clamp((int)(baseColor.G * brightnessFactor), 0, 255);
                int b = Math.Clamp((int)(baseColor.B * brightnessFactor), 0, 255);
                Color = new Color(r, g, b, 255);
            }

            else
            {
                Color = baseColor;
            }

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
