using System.Numerics;
using Ation.Common;

namespace Ation.Simulation
{
    public class SimulationGrid
    {
        private Material?[,] grid;
        private readonly int width;
        private readonly int height;

        public SimulationGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            grid = new Material?[height, width];
        }

        public bool IsValidCell(int x, int y) =>
            x >= 0 && y >= 0 && x < width && y < height;

        public bool IsEmpty(int x, int y) =>
            IsValidCell(x, y) && grid[y, x] == null;

        public bool Is<T>(int x, int y) where T : Material =>
            IsValidCell(x, y) && grid[y, x] is T;

        public Material? Get(int x, int y)
        {
            if (!IsValidCell(x, y)) return null;
            return grid[y, x];
        }

        public void Set(int x, int y, Material? m)
        {
            if (!IsValidCell(x, y)) return;

            grid[y, x] = m;

            if (m != null)
            {
                m.gridPos = new Vector2(x, y);
                m.worldPos = Utils.GridToWorld(m.gridPos);
            }
        }



        public void Clear(int x, int y)
        {
            Set(x, y, null);
        }

        public void Swap(int x1, int y1, int x2, int y2)
        {
            if (!IsValidCell(x1, y1) || !IsValidCell(x2, y2)) return;

            var a = grid[y1, x1];
            var b = grid[y2, x2];

            grid[y1, x1] = b;
            grid[y2, x2] = a;

            if (a != null)
            {
                a.gridPos = new Vector2(x2, y2);
                a.worldPos = Utils.GridToWorld(a.gridPos);
            }

            if (b != null)
            {
                b.gridPos = new Vector2(x1, y1);
                b.worldPos = Utils.GridToWorld(b.gridPos);
            }
        }


        public int Count()
        {
            int count = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (grid[y, x] != null)
                        count++;
            return count;
        }

        public (int Width, int Height) Size => (width, height);


        public void ClearFlags()
        {
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    var m = Get(x, y);
                    if (m != null)
                        m.UpdatedThisFrame = false;
                }
            }
        }
        public void ResetFlags()
        {
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    var m = Get(x, y);
                    if (m != null)
                        m.UpdatedThisFrame = false;
                }
            }
        }


        public void DecayInactiveFlags()
        {
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    var m = Get(x, y);
                    if (m != null && !m.UpdatedThisFrame)
                        m.IsActive = false;
                }
            }
        }
    }
}
