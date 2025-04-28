using System.Numerics;
using Raylib_cs;
using Ation.Common;
using Ation.Simulation;

namespace Ation.Game
{
    public class LegacyGameScene : Scene
    {
        private FallingSandSim sim;
        private Tool selectedTool = Tool.Material;
        private MaterialType selectedMaterial = MaterialType.Sand;

        private Vector2 previousMousePos = Vector2.Zero;
        private bool firstFrame = true;
        private int brushRadius = 3;
        private const int minBrushRadius = 1;
        private const int maxBrushRadius = 20;

        private enum Tool { Material, Wand }

        private static readonly Dictionary<KeyboardKey, MaterialType?> materialBindings = new()
        {
            { KeyboardKey.One, MaterialType.Sand },
            { KeyboardKey.Two, MaterialType.Water },
            { KeyboardKey.Three, MaterialType.Wall },
            { KeyboardKey.Four, MaterialType.Smoke },
            { KeyboardKey.Zero, MaterialType.Eraser }, // Eraser
        };

        public LegacyGameScene()
        {
            int cols = Variables.WindowWidth / Variables.PixelSize;
            int rows = Variables.WindowHeight / Variables.PixelSize;
            sim = new FallingSandSim(cols, rows);
        }

        public override void ProcessInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Zero) || Raylib.IsKeyPressed(KeyboardKey.Kp0))
                selectedTool = Tool.Wand;

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

            Vector2 mouse = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mouse.X, (int)mouse.Y, brushRadius * Variables.PixelSize, Color.Red);
        }
    }
}
