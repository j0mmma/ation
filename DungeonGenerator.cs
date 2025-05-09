using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Ation.Common;
using Raylib_cs;
using Ation.Simulation;

namespace Ation.GameWorld;

public static class DungeonGenerator
{
    public static void GenerateAndSave(string filePath, int chunkSize, int maxWorldSize)
    {
        int width = chunkSize * (maxWorldSize * 2 + 1);
        int height = chunkSize * (maxWorldSize * 2 + 1);
        var grid = GenerateDungeon(width, height);

        var materials = new List<SavedMaterial>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                MaterialType type = grid[x, y];
                if (type != MaterialType.Empty)
                {
                    materials.Add(new SavedMaterial
                    {
                        X = x,
                        Y = y,
                        Type = type.ToString()
                    });
                }
            }

        var json = System.Text.Json.JsonSerializer.Serialize(materials);
        File.WriteAllText(filePath, json);
    }

    private static MaterialType[,] GenerateDungeon(int width, int height, int roomCount = 12)
    {
        var grid = new MaterialType[width, height];
        var rng = new Random();
        var rooms = new List<Rectangle>();

        // Fill all solid
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = MaterialType.Wood; // Replace with Stone if preferred

        // Generate rooms
        for (int i = 0; i < roomCount; i++)
        {
            int w = rng.Next(6, 12);
            int h = rng.Next(6, 12);
            int x = rng.Next(1, width - w - 1);
            int y = rng.Next(1, height - h - 1);

            var room = new Rectangle(x, y, w, h);
            rooms.Add(room);

            for (int rx = x; rx < x + w; rx++)
                for (int ry = y; ry < y + h; ry++)
                    grid[rx, ry] = MaterialType.Empty;
        }

        // Connect rooms
        for (int i = 1; i < rooms.Count; i++)
        {
            var prev = rooms[i - 1];
            var curr = rooms[i];

            int ax = (int)(prev.X + prev.Width / 2);
            int ay = (int)(prev.Y + prev.Height / 2);
            int bx = (int)(curr.X + curr.Width / 2);
            int by = (int)(curr.Y + curr.Height / 2);

            if (rng.Next(2) == 0)
            {
                CarveHLine(grid, ax, bx, ay);
                CarveVLine(grid, ay, by, bx);
            }
            else
            {
                CarveVLine(grid, ay, by, ax);
                CarveHLine(grid, ax, bx, by);
            }
        }

        return grid;
    }

    private static void CarveHLine(MaterialType[,] grid, int x0, int x1, int y)
    {
        for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
            grid[x, y] = MaterialType.Empty;
    }

    private static void CarveVLine(MaterialType[,] grid, int y0, int y1, int x)
    {
        for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
            grid[x, y] = MaterialType.Empty;
    }
}
