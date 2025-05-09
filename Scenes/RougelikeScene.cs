using System.Numerics;
using Raylib_cs;
using Ation.Common;
using Ation.Simulation;
using Ation.GameWorld;
using Ation.Entities;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;

namespace Ation.Game
{
    public class RougelikeScene : Scene
    {
        private int levelCounter = 0;
        private readonly World world;
        private readonly FallingSandSim sim;
        private Renderer renderer;
        private bool firstFrame = true;
        private Camera2D camera;
        private readonly EntityManager entityManager;
        private readonly List<BaseSystem> systems;
        private Entity playerEntity;


        private enum Tool { Material, Wand }


        public RougelikeScene()
        {
            entityManager = new EntityManager();
            playerEntity = entityManager.CreatePlayer(new Vector2(-15, 0));
            var item = entityManager.CreateItem(new Vector2(15, -10));
            systems = new List<BaseSystem>
            {
                new StateSystem(),
                new PlayerInputSystem(),
                new GravitySystem(),
                new MovementIntentSystem(),
                new CollisionSystem()
            };


            world = new World(Variables.ChunkSize);
            sim = new FallingSandSim(world);

            camera = new Camera2D
            {
                Target = new Vector2(Variables.ChunkSize * Variables.PixelSize / 2f, Variables.ChunkSize * Variables.PixelSize / 2f),
                Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
                Zoom = 1.0f,
                Rotation = 0f
            };
            //DungeonGenerator.GenerateAndSave("Assets/test_level.json", Variables.ChunkSize, world.maxWorldSize);
            LevelIO.Load("Assets/test_level_old.json", world);
            //LevelIO.Load("Assets/test_level.json", world);
            renderer = new Renderer(entityManager, world);

        }

        public override void ProcessInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F5))
                LevelIO.Save($"Assets/save_{levelCounter++}.json", world);

            if (Raylib.IsKeyPressed(KeyboardKey.F9))
            {
                world.chunks.Clear();
                LevelIO.Load("Assets/test_level.json", world);
            }


            Vector2 mouseScreen = Raylib.GetMousePosition();
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);

            // if (Raylib.IsMouseButtonDown(MouseButton.Right))
            // {
            //     Vector2 gridPos = Utils.WorldToGrid(mouseWorld);
            //     int x = (int)gridPos.X;
            //     int y = (int)gridPos.Y;

            //     sim.Explode(x, y, radius: 15, force: 400f);
            // }

            // if (Raylib.IsMouseButtonDown(MouseButton.Left) && selectedTool == Tool.Material)
            // {
            //     float distance = Vector2.Distance(previousMousePos, mouseWorld);
            //     int steps = Math.Max(1, (int)(distance / (Variables.PixelSize / 2)));

            //     for (int i = 0; i <= steps; i++)
            //     {
            //         Vector2 pos = Vector2.Lerp(previousMousePos, mouseWorld, (float)i / steps);

            //         if (selectedMaterial == MaterialType.Empty)
            //             sim.ClearMaterials(pos, brushRadius);
            //         else
            //             sim.AddMaterial(pos, selectedMaterial, brushRadius);
            //     }
            // }

        }


        public override void Update(float dt)
        {
            sim.Update(dt);
            world.RemoveEmptyChunks();
            foreach (var system in systems)
                system.Update(entityManager, dt, world);

        }

        public override void Render()
        {
            var renderableChunks = GetRenderableChunks();
            Raylib.BeginMode2D(camera);

            sim.Render(renderableChunks);


            int sizePx = Variables.ChunkSize * Variables.PixelSize;
            int totalChunks = world.maxWorldSize * 2 + 1;
            int totalSizePx = totalChunks * sizePx;

            int topLeftX = -world.maxWorldSize * sizePx;
            int topLeftY = -world.maxWorldSize * sizePx;

            Raylib.DrawRectangleLines(topLeftX, topLeftY, totalSizePx, totalSizePx, Color.Green);


            renderer.Render();
            Raylib.EndMode2D();

            Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
            Raylib.DrawText($"Particles: {sim.CountMaterials()}", 12, 35, 20, Color.Black);
            Raylib.DrawText($"Chunks: {world.ChunkCount()}", 12, 110, 20, Color.Black);
            Raylib.DrawText($"Renderable chunks: {renderableChunks.Count()}", 12, 130, 20, Color.Black);
            if (entityManager.TryGetComponent(playerEntity, out StateComponent state))
            {
                int y = 160; // Starting Y offset
                Raylib.DrawText($"States:", 12, y, 20, Color.DarkGray); y += 22;
                Raylib.DrawText($"InLiquid: {state.IsInLiquid}", 12, y, 20, Color.DarkGray); y += 20;
                Raylib.DrawText($"InLava:   {state.IsInLava}", 12, y, 20, Color.DarkGray); y += 20;
                Raylib.DrawText($"OnFire:   {state.IsOnFire}", 12, y, 20, Color.DarkGray); y += 20;
                Raylib.DrawText($"FireTime: {state.FireDuration:0.00}", 12, y, 20, Color.DarkGray);
            }



            Vector2 mouse = Raylib.GetMousePosition();

            Vector2 mouseScreen = Raylib.GetMousePosition();
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);
            int gridX = (int)(mouseWorld.X / Variables.PixelSize);
            int gridY = (int)(mouseWorld.Y / Variables.PixelSize);

            Raylib.DrawText($"X:{gridX} Y:{gridY}", (int)mouseScreen.X + 12, (int)mouseScreen.Y + 12, 16, Color.DarkGray);

        }


        private List<Chunk> GetRenderableChunks()
        {
            Vector2 topLeft = Raylib.GetScreenToWorld2D(Vector2.Zero, camera);
            Vector2 bottomRight = Raylib.GetScreenToWorld2D(new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), camera);

            float viewLeft = topLeft.X;
            float viewTop = topLeft.Y;
            float viewRight = bottomRight.X;
            float viewBottom = bottomRight.Y;

            var result = new List<Chunk>();
            foreach (var chunk in world.GetAllChunks())
            {
                float chunkLeft = chunk.ChunkX * Variables.ChunkSize * Variables.PixelSize;
                float chunkTop = chunk.ChunkY * Variables.ChunkSize * Variables.PixelSize;
                float chunkRight = chunkLeft + Variables.ChunkSize * Variables.PixelSize;
                float chunkBottom = chunkTop + Variables.ChunkSize * Variables.PixelSize;

                // Keep the chunk if it overlaps the view
                bool overlaps =
                    chunkRight > viewLeft &&
                    chunkLeft < viewRight &&
                    chunkBottom > viewTop &&
                    chunkTop < viewBottom;

                if (overlaps)
                    result.Add(chunk);
            }

            return result;
        }





    }
}
