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
        private Camera2D camera;
        private readonly EntityManager entityManager;
        private readonly List<BaseSystem> systems;
        private Entity playerEntity;


        public RougelikeScene()
        {


            camera = new Camera2D
            {
                Target = new Vector2(Variables.ChunkSize * Variables.PixelSize / 2f, Variables.ChunkSize * Variables.PixelSize / 2f),
                Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f),
                Zoom = 1.0f,
                Rotation = 0f
            };

            entityManager = new EntityManager();
            playerEntity = entityManager.CreatePlayer(new Vector2(-15, 0));
            var item = entityManager.CreateItem(new Vector2(-30, -10));

            entityManager.CreateEnemy(new Vector2(100, 0));

            systems = new List<BaseSystem>
            {
                new StateSystem(),
                new PlayerInputSystem(camera),
                new GravitySystem(),
                new MovementIntentSystem(),
                new CollisionSystem(),
                new ProjectileSystem(),
                new PickupSystem(),
                new ItemUseSystem(camera),
                //new DamageSystem(),
            };


            world = new World(Variables.ChunkSize);
            sim = new FallingSandSim(world);

            LevelIO.Load("Assets/test_level_old.json", world);
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

            // draw line to cursor
            if (entityManager.TryGetComponent(playerEntity, out TransformComponent playerTransform) &&
                        entityManager.TryGetComponent(playerEntity, out ColliderComponent playerCol))
            {
                Vector2 playerOrigin = playerTransform.Position + playerCol.Offset + playerCol.Size * 0.5f;

                Vector2 cursorWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

                Vector2 originPx = playerOrigin * Variables.PixelSize;
                Vector2 cursorPx = cursorWorld;

                Raylib.DrawLineV(originPx, cursorPx, Color.Red);

                Raylib.DrawCircleV(originPx, 2, Color.Blue);     // Player center
                Raylib.DrawCircleV(cursorPx, 2, Color.Yellow);   // Cursor position


                //Raylib.DrawText($"X:{cursorPx.X} Y:{cursorPx.Y}", (int)cursorPx.X + 12, (int)cursorPx.Y + 12, 16, Color.DarkGray);
            }




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

            if (entityManager.TryGetComponent(playerEntity, out InventoryComponent inventory))
            {
                int y = 260;
                Raylib.DrawText("Inventory:", 12, y, 20, Color.DarkGray);
                y += 22;

                for (int i = 0; i < inventory.Slots.Length; i++)
                {
                    var item = inventory.Slots[i];
                    string itemText = item != null ? $"item_{item.Id}" : "(empty)";
                    Color color = i == inventory.SelectedIndex ? Color.Yellow : Color.Gray;

                    Raylib.DrawText($"[{i}] {itemText}", 12, y, 20, color);
                    y += 20;
                }
            }



            Vector2 mouseScreen = Raylib.GetMousePosition();
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);
            int gridX = (int)(mouseWorld.X / Variables.PixelSize);
            int gridY = (int)(mouseWorld.Y / Variables.PixelSize);


            int xPx = (int)mouseWorld.X;
            int yPx = (int)mouseWorld.Y;

            Raylib.DrawText($"X:{gridX} Y:{gridY} | {xPx}, {yPx}", (int)mouseScreen.X + 12, (int)mouseScreen.Y + 12, 16, Color.DarkGray);

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
