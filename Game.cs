using System.Numerics;
using Raylib_cs;

using Ation.Systems;
using Ation.Entities;
using Ation.Simulation;
using Ation.Common;

namespace Ation.Game
{
    class Game
    {
        private const int WINDOW_W = 500;
        private const int WINDOW_H = 500;

        private static ParticleSim particleSim = new ParticleSim();
        private enum Tool
        {
            Material,
            Wand
        }

        private static Tool selectedTool = Tool.Material;
        //private static Func<Simulation.Material>? selectedMaterialFactory = () => new Sand();
        private static readonly Dictionary<KeyboardKey, Func<Simulation.Material>?> materialBindings = new()
        // {
        //     { KeyboardKey.One, () => new Sand() },
        //     { KeyboardKey.Two, () => new Water() },
        //    // { KeyboardKey.Three, () => new Solid() },
        //     { KeyboardKey.Four, () => new Steam() },
        //     { KeyboardKey.Five, () => null }, // Eraser
        // };
        private static Player player = new Player(particleSim);

        private static Vector2 previousMousePos = Vector2.Zero;
        private static bool firstFrame = true;

        private static int brushRadius = 3; // default brush radius
        private static readonly int minBrushRadius = 1;
        private static readonly int maxBrushRadius = 20;

        public static void Main()
        {
            Raylib.InitWindow(Variables.WindowWidth, Variables.WindowHeight, "Ation");
            Raylib.SetTargetFPS(60);

            //particleSim.LoadTestLevel("test_level.json");


            while (!Raylib.WindowShouldClose())
            {
                ProcessInput();
                Update(Raylib.GetFrameTime());
                Render();
            }

            Raylib.CloseWindow();
        }

        public static void ProcessInput()
        {
            // Movement
            if (Raylib.IsKeyDown(KeyboardKey.A)) player.MoveLeft();
            else if (Raylib.IsKeyDown(KeyboardKey.D)) player.MoveRight();
            else player.StopHorizontal();

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                player.Jump();

            // Tool selection
            if (Raylib.IsKeyPressed(KeyboardKey.Zero) || Raylib.IsKeyPressed(KeyboardKey.Kp0))
                selectedTool = Tool.Wand;

            foreach (var (key, factory) in materialBindings)
            {
                if (Raylib.IsKeyPressed(key))
                {
                    selectedTool = Tool.Material;
                    selectedMaterialFactory = factory;
                    break;
                }
            }


            // Adjust brush size
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

            // Draw particles if in brush mode
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && selectedTool == Tool.Material)
            {
                float distance = Vector2.Distance(previousMousePos, mousePos);
                int steps = Math.Max(1, (int)(distance / (Variables.PixelSize / 2)));

                for (int i = 0; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector2 interpolatedPos = Vector2.Lerp(previousMousePos, mousePos, t);
                    if (selectedMaterialFactory == null)
                    {
                        // Eraser
                        particleSim.ClearParticles(interpolatedPos, brushRadius);
                    }
                    else
                    {
                        particleSim.AddParticle(interpolatedPos, selectedMaterialFactory(), radius: brushRadius);
                    }


                }
            }

            previousMousePos = mousePos;
        }


        public static void Update(float dt)
        {
            player.WandEnabled = (selectedTool == Tool.Wand);

            List<ICollider> colliders = new() { player };
            //player.Update(dt);
            particleSim.Update(dt);
        }

        public static void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);

            particleSim.Render();
            player.Render(); // draw player on top

            string label = selectedMaterialFactory?.Invoke()?.DisplayName ?? "Null";
            Raylib.DrawText($"Material: {label}", 12, 60, 20, Color.Black);

            Raylib.DrawText($"Brush Size: {brushRadius}", 12, 85, 20, Color.Black);

            // Draw brush outline
            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mousePos.X, (int)mousePos.Y, brushRadius * Variables.PixelSize, Color.Red);
            Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
            Raylib.DrawText($"Particles: {particleSim.CountParticles()}", 12, 35, 20, Color.Black);

            Raylib.EndDrawing();
        }
    }
}
