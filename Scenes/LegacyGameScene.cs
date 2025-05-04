using System.Numerics;
using Raylib_cs;
using Ation.Common;
using Ation.Simulation;
using Ation.GameWorld;

namespace Ation.Game
{
    public class LegacyGameScene : Scene
    {
        private readonly World world;
        private readonly FallingSandSim sim;

        private Tool selectedTool = Tool.Material;
        private MaterialType selectedMaterial = MaterialType.Sand;

        private Vector2 previousMousePos = Vector2.Zero;
        private bool firstFrame = true;
        private int brushRadius = 3;
        private const int minBrushRadius = 1;
        private const int maxBrushRadius = 20;
        private Camera2D camera;

        private enum Tool { Material, Wand }

        private static readonly Dictionary<KeyboardKey, MaterialType?> materialBindings = new()
        {
            { KeyboardKey.One, MaterialType.Sand },
            { KeyboardKey.Two, MaterialType.Water },
            { KeyboardKey.Three, MaterialType.Wood },
            { KeyboardKey.Five, MaterialType.Acid },
            { KeyboardKey.Six, MaterialType.Smoke },
            { KeyboardKey.Seven, MaterialType.Fire },
            { KeyboardKey.Zero, MaterialType.Eraser },
        };

        public LegacyGameScene()
        {
            world = new World(Variables.ChunkSize);
            sim = new FallingSandSim(world);

            camera = new Camera2D
            {
                Target = new Vector2(Variables.ChunkSize * Variables.PixelSize / 2f, Variables.ChunkSize * Variables.PixelSize / 2f),
                Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
                Zoom = 1.0f,
                Rotation = 0f
            };
        }

        public override void ProcessInput()
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 mouseWorld = Raylib.GetMousePosition();
                Vector2 gridPos = Utils.WorldToGrid(mouseWorld);
                int x = (int)gridPos.X;
                int y = (int)gridPos.Y;

                sim.Explode(x, y, radius: 15, force: 500f);
            }

            float cameraSpeed = 500f * Raylib.GetFrameTime();
            if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Target.Y -= cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.S)) camera.Target.Y += cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.A)) camera.Target.X -= cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.D)) camera.Target.X += cameraSpeed;

            foreach (var (key, matType) in materialBindings)
            {
                if (Raylib.IsKeyPressed(key))
                {
                    selectedTool = Tool.Material;
                    selectedMaterial = matType ?? MaterialType.Empty;
                    break;
                }
            }

            float scroll = Raylib.GetMouseWheelMove();
            if (scroll != 0)
            {
                brushRadius += (int)scroll;
                brushRadius = Math.Clamp(brushRadius, minBrushRadius, maxBrushRadius);
            }

            Vector2 mousePos = Raylib.GetMousePosition();

            if (firstFrame)
            {
                previousMousePos = mousePos;
                firstFrame = false;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && selectedTool == Tool.Material)
            {
                float distance = Vector2.Distance(previousMousePos, mousePos);
                int steps = Math.Max(1, (int)(distance / (Variables.PixelSize / 2)));

                for (int i = 0; i <= steps; i++)
                {
                    Vector2 pos = Vector2.Lerp(previousMousePos, mousePos, (float)i / steps);

                    if (selectedMaterial == MaterialType.Empty)
                        sim.ClearMaterials(pos, brushRadius);
                    else
                        sim.AddMaterial(pos, selectedMaterial, brushRadius);
                }
            }

            previousMousePos = mousePos;
        }

        public override void Update(float dt)
        {
            sim.Update(dt);
        }

        public override void Render()
        {
            sim.Render();

            Raylib.DrawText($"Material: {selectedMaterial}", 12, 60, 20, Color.Black);
            Raylib.DrawText($"Brush Size: {brushRadius}", 12, 85, 20, Color.Black);
            Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
            Raylib.DrawText($"Particles: {sim.CountMaterials()}", 12, 35, 20, Color.Black);
            Raylib.DrawText($"Chunks: {world.ChunkCount()}", 12, 110, 20, Color.Black);

            Vector2 mouse = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mouse.X, (int)mouse.Y, brushRadius * Variables.PixelSize, Color.Red);
        }
    }
}
