using System.Numerics;
using Raylib_cs;

namespace Ation.Simulation
{
    public abstract class Solid : Material
    {
        public Solid(Vector2 worldPos) : base(worldPos) { }
    }
}