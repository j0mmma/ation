using System.Numerics;
using System.Linq;

using Ation.Common;
using Ation.GameWorld;

using Raylib_cs;

namespace Ation.Simulation
{
    class FallingSandSim
    {
        private readonly IMaterialContext context;
        public static Vector2 Gravity = new Vector2(0, 2000f);
        private int frameCounter = 0;

        public FallingSandSim(IMaterialContext world)
        {
            context = world;
        }

        public void Update(float dt)
        {
            context.ResetFlags();
            frameCounter++;
            bool flipX = frameCounter % 2 == 0;

            void StepAll(int width, int height, Func<int, int, Material?> get)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    if (flipX)
                    {
                        for (int x = width - 1; x >= 0; x--)
                        {
                            var m = get(x, y);
                            if (m == null || m.UpdatedThisFrame) continue;
                            m.Step(context);
                        }
                    }
                    else
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var m = get(x, y);
                            if (m == null || m.UpdatedThisFrame) continue;
                            m.Step(context);
                        }
                    }
                }
            }

            if (context is IChunkedMaterialContext chunked)
            {
                foreach (var chunk in chunked.GetAllChunks().ToList())
                {
                    var (w, h) = chunk.Size;
                    int offsetX = chunk.ChunkX * w;
                    int offsetY = chunk.ChunkY * h;

                    StepAll(w, h, (x, y) =>
                    {
                        var m = chunk.Grid.Get(x, y);
                        if (m != null)
                        {
                            m.gridPos = new Vector2(x + offsetX, y + offsetY);
                            m.worldPos = Utils.GridToWorld(m.gridPos);
                        }
                        return m;
                    });
                }
            }
            else if (context is SimulationGrid grid)
            {
                var (w, h) = grid.Size;
                StepAll(w, h, grid.Get);
            }

        }




        public void Render(List<Chunk> visibleChunks)
        {
            foreach (var chunk in visibleChunks)
            {
                int offsetX = chunk.ChunkX * chunk.Size.Width;
                int offsetY = chunk.ChunkY * chunk.Size.Height;
                var (width, height) = chunk.Size;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var m = chunk.Grid.Get(x, y);
                        if (m == null) continue;

                        int worldX = (offsetX + x) * Variables.PixelSize;
                        int worldY = (offsetY + y) * Variables.PixelSize;

                        Raylib.DrawRectangle(worldX, worldY, Variables.PixelSize, Variables.PixelSize, m.Color);
                    }
                }

                int chunkPixelX = offsetX * Variables.PixelSize;
                int chunkPixelY = offsetY * Variables.PixelSize;
                int widthPx = width * Variables.PixelSize;
                int heightPx = height * Variables.PixelSize;

                Raylib.DrawRectangleLines(chunkPixelX, chunkPixelY, widthPx, heightPx, Color.Red);
            }
        }





        public void Explode(int cx, int cy, int radius, float force)
        {
            var explosion = new Explosion(context, cx, cy, radius, force);
            explosion.Enact();
        }

        public void AddMaterial(Vector2 worldPos, MaterialType type, int radius = 3)
        {
            var gridPos = Utils.WorldToGrid(worldPos);
            int cx = (int)gridPos.X;
            int cy = (int)gridPos.Y;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int gx = cx + x;
                    int gy = cy + y;
                    if (x * x + y * y > radius * radius) continue;
                    if (!context.IsValidCell(gx, gy)) continue;
                    if (context.IsEmpty(gx, gy))
                    {
                        var world = Utils.GridToWorld(new Vector2(gx, gy));
                        context.Set(gx, gy, MaterialFactory.Create(type, world));
                    }
                }
            }
        }

        public void ClearMaterials(Vector2 worldPos, int radius = 3)
        {
            var gridPos = Utils.WorldToGrid(worldPos);
            int cx = (int)gridPos.X;
            int cy = (int)gridPos.Y;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int gx = cx + x;
                    int gy = cy + y;
                    if (x * x + y * y > radius * radius) continue;
                    if (context.IsValidCell(gx, gy))
                        context.Clear(gx, gy);
                }
            }
        }

        public int CountMaterials() => context.Count();
    }
}
