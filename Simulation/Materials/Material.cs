using Raylib_cs;
using Ation.Simulation;

namespace Ation.Simulation
{
    abstract class Material
    {
        public abstract string DisplayName { get; }

        public abstract void step(Simulation.SimulationGrid grid);


    }
}