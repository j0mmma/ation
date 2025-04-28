using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class ImmovableSolid : Material
    {
        public ImmovableSolid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(SimulationGrid grid)
        {
            // Immovable solids don't need to do anything every frame
            UpdatedThisFrame = true;
            IsActive = false;
        }

        public override bool ActOnNeighbor(Material neighbor, int targetX, int targetY, SimulationGrid grid)
        {
            // Immovable solids don't interact with neighbors actively
            return false;
        }
    }
}
