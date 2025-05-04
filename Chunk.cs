using Ation.Simulation;

namespace Ation.GameWorld
{
    public class Chunk
    {
        public readonly int ChunkX;
        public readonly int ChunkY;
        public readonly SimulationGrid Grid;

        public Chunk(int chunkX, int chunkY, int size)
        {
            ChunkX = chunkX;
            ChunkY = chunkY;
            Grid = new SimulationGrid(size, size);
        }

        public (int Width, int Height) Size => Grid.Size;
    }
}
