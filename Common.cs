using System.Numerics;

namespace Ation.Common
{
    static class Variables
    {
        public static int WindowWidth = 1000;
        public static int WindowHeight = 500;
        public static int WorldWidth = WindowWidth;
        public static int WorldHeight = WindowHeight;
        public static int PixelSize = 4;


        public static float BaseGravity = 2;
    }


    static class Utils
    {
        public static bool IsPositionInsideWindow(Vector2 pos)
        {
            return pos.X >= 0 && pos.X < Variables.WindowWidth &&
                   pos.Y >= 0 && pos.Y < Variables.WindowHeight;
        }

        public static bool IsPositionInsideWorld(Vector2 pos)
        {
            return pos.X >= 0 && pos.X < Variables.WorldWidth &&
                   pos.Y >= 0 && pos.Y < Variables.WorldHeight;
        }

        public static bool IsInGridBounds(Vector2 gridPos)
        {
            int gridWidth = Variables.WorldWidth / Variables.PixelSize;
            int gridHeight = Variables.WorldHeight / Variables.PixelSize;

            return gridPos.X >= 0 && gridPos.X < gridWidth &&
                   gridPos.Y >= 0 && gridPos.Y < gridHeight;
        }



        // World (pixels) -> Grid (indices as Vector2)
        public static Vector2 WorldToGrid(Vector2 worldPos)
        {
            return new Vector2(
                (int)(worldPos.X / Variables.PixelSize),
                (int)(worldPos.Y / Variables.PixelSize)
            );
        }

        // Grid (indices) -> World (top-left pixel position)
        public static Vector2 GridToWorld(Vector2 gridPos)
        {
            return new Vector2(
                gridPos.X * Variables.PixelSize,
                gridPos.Y * Variables.PixelSize
            );
        }
    }
}
