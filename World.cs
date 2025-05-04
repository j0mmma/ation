using System.Collections.Generic;
using System.Numerics;

using Ation.Simulation;

namespace Ation.GameWorld
{
    public class World : IChunkedMaterialContext
    {
        private readonly int chunkSize;
        private readonly Dictionary<(int, int), Chunk> chunks = new();
        private readonly int maxWorldSize = 10; // Maximum number of chunks in each direction


        public World(int chunkSize)
        {
            this.chunkSize = chunkSize;

            // Manually create chunk at (0,0)
            var chunk00 = new Chunk(0, 0, chunkSize);
            chunks[(0, 0)] = chunk00;

            // Manually create chunk at (1,0)
            var chunk10 = new Chunk(1, 0, chunkSize);
            chunks[(1, 0)] = chunk10;
        }


        private (Chunk chunk, int localX, int localY)? Resolve(int x, int y)
        {
            int chunkX = Math.DivRem(x, chunkSize, out int localX);
            int chunkY = Math.DivRem(y, chunkSize, out int localY);
            if (localX < 0) { chunkX--; localX += chunkSize; }
            if (localY < 0) { chunkY--; localY += chunkSize; }

            // Check bounds before proceeding
            if (Math.Abs(chunkX) >= maxWorldSize || Math.Abs(chunkY) >= maxWorldSize)
                return null;

            var key = (chunkX, chunkY);
            if (!chunks.TryGetValue(key, out var chunk))
            {
                chunk = new Chunk(chunkX, chunkY, chunkSize);
                chunks[key] = chunk;
            }

            return (chunk, localX, localY);
        }


        public Material? Get(int x, int y)
        {
            var resolved = Resolve(x, y);
            return resolved?.chunk.Grid.Get(resolved.Value.localX, resolved.Value.localY);
        }

        public void Set(int x, int y, Material? m)
        {
            var resolved = Resolve(x, y);
            resolved?.chunk.Grid.Set(resolved.Value.localX, resolved.Value.localY, m);
        }

        public void Clear(int x, int y)
        {
            var resolved = Resolve(x, y);
            resolved?.chunk.Grid.Clear(resolved.Value.localX, resolved.Value.localY);
        }

        public void Swap(int x1, int y1, int x2, int y2)
        {
            var a = Resolve(x1, y1);
            var b = Resolve(x2, y2);
            if (a == null || b == null) return;

            var mA = a.Value.chunk.Grid.Get(a.Value.localX, a.Value.localY);
            var mB = b.Value.chunk.Grid.Get(b.Value.localX, b.Value.localY);

            a.Value.chunk.Grid.Set(a.Value.localX, a.Value.localY, mB);
            b.Value.chunk.Grid.Set(b.Value.localX, b.Value.localY, mA);
        }

        public bool IsValidCell(int x, int y) => true;
        public bool IsEmpty(int x, int y) => Get(x, y) == null;

        public int Count()
        {
            int total = 0;
            foreach (var chunk in chunks.Values)
                total += chunk.Grid.Count();
            return total;
        }

        public void ResetFlags()
        {
            foreach (var chunk in chunks.Values)
            {
                chunk.Grid.ResetFlags();
            }
        }
        public int ChunkCount() => chunks.Count;

        public IEnumerable<Chunk> GetAllChunks()
        {
            return chunks.Values;
        }
    }
}
