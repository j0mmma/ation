using System.Collections.Generic;
using System.Numerics;

using Ation.Simulation;

namespace Ation.GameWorld
{
    public class World : IChunkedMaterialContext
    {
        private readonly int chunkSize;
        public readonly Dictionary<(int, int), Chunk> chunks = new();
        public readonly int maxWorldSize = 2; // Maximum number of chunks in each direction


        public World(int chunkSize)
        {
            this.chunkSize = chunkSize;

            // Manually create chunk at (0,0)
            var chunk00 = new Chunk(0, 0, chunkSize);
            chunks[(0, 0)] = chunk00;
        }


        private (Chunk chunk, int localX, int localY)? Resolve(int x, int y)
        {
            int chunkX = Math.DivRem(x, chunkSize, out int localX);
            int chunkY = Math.DivRem(y, chunkSize, out int localY);
            if (localX < 0) { chunkX--; localX += chunkSize; }
            if (localY < 0) { chunkY--; localY += chunkSize; }

            // Correct world bounds check (inclusive range)
            if (chunkX < -maxWorldSize || chunkX > maxWorldSize ||
                chunkY < -maxWorldSize || chunkY > maxWorldSize)
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

        public void RemoveEmptyChunks()
        {
            var keysToRemove = new List<(int, int)>();

            foreach (var (key, chunk) in chunks)
            {
                if (chunk.Grid.Count() == 0)
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
                chunks.Remove(key);
        }


        private (Chunk? chunk, int localX, int localY) GetChunkAndLocalCoords(int worldX, int worldY)
        {
            int chunkX = Math.DivRem(worldX, chunkSize, out int localX);
            int chunkY = Math.DivRem(worldY, chunkSize, out int localY);
            if (localX < 0) { chunkX--; localX += chunkSize; }
            if (localY < 0) { chunkY--; localY += chunkSize; }

            var key = (chunkX, chunkY);
            chunks.TryGetValue(key, out var chunk);
            return (chunk, localX, localY);
        }

        public bool IsCollidableAt(int worldX, int worldY)
        {
            var (chunk, localX, localY) = GetChunkAndLocalCoords(worldX, worldY);
            if (chunk == null || !chunk.Grid.IsValidCell(localX, localY))
                return false;

            var material = chunk.Grid.Get(localX, localY);
            return material != null && material.IsCollidable;
        }

        public IEnumerable<Chunk> GetAllChunks()
        {
            return chunks.Values;
        }

        public void Explode(int x, int y, int radius, float force)
        {
            var resolved = Resolve(x, y);
            if (resolved == null) return;

            var (chunk, localX, localY) = resolved.Value;
            Console.WriteLine($"[World.Explode] global ({x}, {y}) → chunk ({chunk.ChunkX}, {chunk.ChunkY}) local ({localX}, {localY})");

            var explosion = new Explosion(chunk.Grid, localX, localY, radius, force);
            explosion.Enact();
        }

    }
}
