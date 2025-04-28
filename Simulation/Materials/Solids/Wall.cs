using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public class Wall : ImmovableSolid
    {
        public override string DisplayName => "Wall";
        public override MaterialType Type => MaterialType.Wall;

        public Wall(Vector2 pos) : base(pos)
        {
            Color = Raylib_cs.Color.Gray;
            Mass = float.PositiveInfinity; // infinite mass = immovable
        }
    }
}
