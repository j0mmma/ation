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
            if (Raylib.IsKeyPressed(KeyboardKey.One))
                selectedMaterial = ParticleType.Sand;

            if (Raylib.IsKeyPressed(KeyboardKey.Two))
                selectedMaterial = ParticleType.Water;

            if (Raylib.IsKeyPressed(KeyboardKey.Three))
                selectedMaterial = ParticleType.Solid;

            if (Raylib.IsKeyPressed(KeyboardKey.Four))
                selectedMaterial = ParticleType.Steam;

            if (Raylib.IsKeyPressed(KeyboardKey.Five))
                selectedMaterial = ParticleType.Eraser;

            // Scroll wheel to adjust brush radius
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

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
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
            particleSys.Update(dt);
            particleSim.Update(dt);
            //player.Update(dt);
        }

        public static void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);

            particleSim.Render();
            //player.Render(); // draw player on top

            Raylib.DrawText($"Material: {selectedMaterial}", 12, 60, 20, Color.Black);
            Raylib.DrawText($"Brush Size: {brushRadius}", 12, 85, 20, Color.Black);

            // Draw brush outline
            Vector2 mousePos = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mousePos.X, (int)mousePos.Y, brushRadius * Variables.PixelSize, Color.Red);

            Raylib.EndDrawing();
        }
    }
}
