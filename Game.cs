using System.Numerics;
using Raylib_cs;

using Ation.Systems;
using Ation.Entities;
using Ation.ParticleSimulation;
using Ation.Common;

namespace Ation.Game
{
    class Game
    {
        private const int WINDOW_W = 500;
        private const int WINDOW_H = 500;

        private static EntityManager entityManager = new EntityManager();
        private static Renderer renderer = new Renderer(entityManager, Variables.WindowWidth, Variables.WindowHeight);
        private static ParticleSystem particleSys = new ParticleSystem(entityManager);
        private static ParticleSim particleSim = new ParticleSim();
        private enum Tool
        {
            Material,
            Wand
        }

        private static Tool selectedTool = Tool.Material;

        private static ParticleType selectedMaterial = ParticleType.Sand;
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

            particleSim.LoadTestLevel("test_level.json");


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

            if (Raylib.IsKeyPressed(KeyboardKey.One))
            {
                selectedTool = Tool.Material;
                selectedMaterial = ParticleType.Sand;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Two))
            {
                selectedTool = Tool.Material;
                selectedMaterial = ParticleType.Water;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Three))
            {
                selectedTool = Tool.Material;
                selectedMaterial = ParticleType.Solid;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Four))
            {
                selectedTool = Tool.Material;
                selectedMaterial = ParticleType.Steam;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Five))
            {
                selectedTool = Tool.Material;
                selectedMaterial = ParticleType.Eraser;
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
                    particleSim.AddParticle(interpolatedPos, selectedMaterial, radius: brushRadius);
                }
            }

            previousMousePos = mousePos;
        }


        public static void Update(float dt)
        {
            //particleSys.Update(dt);
            player.WandEnabled = (selectedTool == Tool.Wand);

            List<ICollider> colliders = new() { player };
            player.Update(dt);
            particleSim.Update(dt);
        }

        public static void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);

            particleSim.Render();
            player.Render(); // draw player on top

            Raylib.DrawText($"Material: {selectedMaterial}", 12, 60, 20, Color.Black);
            Raylib.DrawText($"Brush Size: {brushRadius}", 12, 85, 20, Color.Black);

            // Draw brush outline
            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mousePos.X, (int)mousePos.Y, brushRadius * Variables.PixelSize, Color.Red);

            Raylib.EndDrawing();
        }
    }
}
