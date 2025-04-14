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

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                Vector2 mousePos = Raylib.GetMousePosition();
                particleSim.AddParticle(mousePos, selectedMaterial, radius: 3);
                Console.WriteLine($"Spawned {selectedMaterial} at: {mousePos}");
            }
        }

        public static void Update(float dt)
        {
            particleSys.Update(dt);
            particleSim.Update(dt);
        }
        public static void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);

            particleSim.Render(); // Draw particles

            // Draw UI (FPS, Particle count already drawn by ParticleSim, now add Material)
            Raylib.DrawText($"Material: {selectedMaterial}", 12, 60, 20, Color.Black);

            Raylib.EndDrawing();
        }
    }
}

