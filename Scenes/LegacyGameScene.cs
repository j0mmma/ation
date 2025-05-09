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
    public class LegacyGameScene : Scene
    {
        private int levelCounter = 0;
        private readonly World world;
        private readonly FallingSandSim sim;
        //private Renderer renderer;

        private Tool selectedTool = Tool.Material;
        private MaterialType selectedMaterial = MaterialType.Sand;

        private Vector2 previousMousePos = Vector2.Zero;
        private bool firstFrame = true;
        private int brushRadius = 3;
        private const int minBrushRadius = 1;
        private const int maxBrushRadius = 20;
        private Camera2D camera;
        //private readonly EntityManager entityManager;
        //private readonly List<BaseSystem> systems;
        //private Entity playerEntity;


        private enum Tool { Material, Wand }

        private static readonly Dictionary<KeyboardKey, MaterialType?> materialBindings = new()
        {
            { KeyboardKey.One, MaterialType.Sand },
            { KeyboardKey.Two, MaterialType.Water },
            { KeyboardKey.Three, MaterialType.Wood },
            { KeyboardKey.Five, MaterialType.Acid },
            { KeyboardKey.Six, MaterialType.Steam },
            { KeyboardKey.Seven, MaterialType.Fire },
            { KeyboardKey.Eight, MaterialType.Stone },
            { KeyboardKey.Nine, MaterialType.Lava },
            { KeyboardKey.Zero, MaterialType.Eraser },
        };

        public LegacyGameScene()
        {
            //entityManager = new EntityManager();
            //playerEntity = entityManager.CreatePlayer(new Vector2(-15, 0));
            //var item = entityManager.CreateItem(new Vector2(15, -10));
            // systems = new List<BaseSystem>
            // {
            //     new StateSystem(),
            //     new PlayerInputSystem(camera),
            //     new GravitySystem(),
            //     new MovementIntentSystem(),
            //     new CollisionSystem()
            // };


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
            //LevelIO.Load("Assets/test_level_old.json", world);
            //LevelIO.Load("Assets/test_level.json", world);
            //renderer = new Renderer(entityManager, world);

        }

        public override void ProcessInput()
        {

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                SceneManager.PopScene(); // go back to Main Menu
                return;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.F5))
                LevelIO.Save($"Assets/save_{levelCounter++}.json", world);

            if (Raylib.IsKeyPressed(KeyboardKey.F9))
            {
                world.chunks.Clear();
                //LevelIO.Load("Assets/level0.json", world);
            }


            Vector2 mouseScreen = Raylib.GetMousePosition();
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);

            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 gridPos = Utils.WorldToGrid(mouseWorld);
                int x = (int)gridPos.X;
                int y = (int)gridPos.Y;

                world.Explode(x, y, radius: 15, force: 400f);
            }

            float cameraSpeed = 800f * Raylib.GetFrameTime();
            if (Raylib.IsKeyDown(KeyboardKey.Up)) camera.Target.Y -= cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.Down)) camera.Target.Y += cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.Left)) camera.Target.X -= cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) camera.Target.X += cameraSpeed;

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

            if (firstFrame)
            {
                previousMousePos = mouseWorld;
                firstFrame = false;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && selectedTool == Tool.Material)
            {
                float distance = Vector2.Distance(previousMousePos, mouseWorld);
                int steps = Math.Max(1, (int)(distance / (Variables.PixelSize / 2)));

                for (int i = 0; i <= steps; i++)
                {
                    Vector2 pos = Vector2.Lerp(previousMousePos, mouseWorld, (float)i / steps);

                    if (selectedMaterial == MaterialType.Empty)
                        sim.ClearMaterials(pos, brushRadius);
                    else
                        sim.AddMaterial(pos, selectedMaterial, brushRadius);
                }
            }

            previousMousePos = mouseWorld;
        }


        public override void Update(float dt)
        {
            sim.Update(dt);
            world.RemoveEmptyChunks();
            //foreach (var system in systems)
            //    system.Update(entityManager, dt, world);

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


            //renderer.Render();
            Raylib.EndMode2D();

            Raylib.DrawText($"Material: {selectedMaterial}", 12, 60, 20, Color.Black);
            Raylib.DrawText($"Brush Size: {brushRadius}", 12, 85, 20, Color.Black);
            Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
            Raylib.DrawText($"Particles: {sim.CountMaterials()}", 12, 35, 20, Color.Black);
            Raylib.DrawText($"Chunks: {world.ChunkCount()}", 12, 110, 20, Color.Black);
            Raylib.DrawText($"Renderable chunks: {renderableChunks.Count()}", 12, 130, 20, Color.Black);
            // if (entityManager.TryGetComponent(playerEntity, out StateComponent state))
            // {
            //     int y = 160; // Starting Y offset
            //     Raylib.DrawText($"States:", 12, y, 20, Color.DarkGray); y += 22;
            //     Raylib.DrawText($"InLiquid: {state.IsInLiquid}", 12, y, 20, Color.DarkGray); y += 20;
            //     Raylib.DrawText($"InLava:   {state.IsInLava}", 12, y, 20, Color.DarkGray); y += 20;
            //     Raylib.DrawText($"OnFire:   {state.IsOnFire}", 12, y, 20, Color.DarkGray); y += 20;
            //     Raylib.DrawText($"FireTime: {state.FireDuration:0.00}", 12, y, 20, Color.DarkGray);
            // }



            DrawMaterialSelectorHUD();
            Vector2 mouse = Raylib.GetMousePosition();
            Raylib.DrawCircleLines((int)mouse.X, (int)mouse.Y, brushRadius * Variables.PixelSize, Color.Red);

            Vector2 mouseScreen = Raylib.GetMousePosition();
            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);
            int gridX = (int)(mouseWorld.X / Variables.PixelSize);
            int gridY = (int)(mouseWorld.Y / Variables.PixelSize);

            Raylib.DrawText($"X:{gridX} Y:{gridY}", (int)mouseScreen.X + 12, (int)mouseScreen.Y + 12, 16, Color.DarkGray);

        }

        private void DrawMaterialSelectorHUD()
        {
            int buttonWidth = 160;
            int buttonHeight = 28;
            int padding = 6;
            int startX = Raylib.GetScreenWidth() - buttonWidth - 20;
            int startY = 20;

            int i = 0;
            foreach (MaterialType type in Enum.GetValues(typeof(MaterialType)))
            {
                int x = startX;
                int y = startY + i * (buttonHeight + padding);
                Rectangle rect = new Rectangle(x, y, buttonWidth, buttonHeight);

                bool hovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
                bool clicked = hovered && Raylib.IsMouseButtonPressed(MouseButton.Left);

                Color bgColor = type == selectedMaterial ? Color.Green :
                                hovered ? Color.Blue : Color.Gray;

                Raylib.DrawRectangleRec(rect, bgColor);
                Raylib.DrawRectangleLinesEx(rect, 2, Color.Black);
                Raylib.DrawText(type.ToString(), x + 8, y + 6, 16, Color.Black);

                if (clicked)
                {
                    selectedMaterial = type;
                    selectedTool = Tool.Material;
                }

                i++;
            }
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
