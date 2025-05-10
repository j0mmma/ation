using Raylib_cs;
using System.Numerics;
using Ation.Common;
using System.IO;

namespace Ation.Game
{
    public class LevelSelectScene : Scene
    {
        private List<string> levelNames = new();
        private const int ButtonWidth = 300;
        private const int ButtonHeight = 30;
        private const int ButtonSpacing = 10;

        public LevelSelectScene()
        {
            var files = Directory.GetFiles("Assets/Levels", "*.json");
            levelNames = files.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        public override void ProcessInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                SceneManager.PopScene(); // go back to Main Menu
                return;
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mouse = Raylib.GetMousePosition();
                int screenWidth = Raylib.GetScreenWidth();
                int screenHeight = Raylib.GetScreenHeight();

                float totalHeight = levelNames.Count * ButtonHeight + (levelNames.Count - 1) * ButtonSpacing;
                float startY = screenHeight / 2f - totalHeight / 2f;
                float startX = screenWidth / 2f - ButtonWidth / 2f;

                for (int i = 0; i < levelNames.Count; i++)
                {
                    Rectangle r = new(startX, startY + i * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);
                    if (Raylib.CheckCollisionPointRec(mouse, r))
                    {
                        string levelPath = $"Assets/Levels/{levelNames[i]}.json";
                        SceneManager.PushScene(new RougelikeScene(levelPath));
                        return;
                    }
                }
            }
        }

        public override void Update(float dt) { }

        public override void Render()
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();

            string title = "Select Level";
            int titleFontSize = 30;
            int titleWidth = Raylib.MeasureText(title, titleFontSize);
            Raylib.DrawText(title, screenWidth / 2 - titleWidth / 2, 50, titleFontSize, Color.Black);

            float totalHeight = levelNames.Count * ButtonHeight + (levelNames.Count - 1) * ButtonSpacing;
            float startY = screenHeight / 2f - totalHeight / 2f;
            float startX = screenWidth / 2f - ButtonWidth / 2f;

            for (int i = 0; i < levelNames.Count; i++)
            {
                Rectangle r = new(startX, startY + i * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);
                bool hovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r);

                Raylib.DrawRectangleRec(r, hovered ? Color.LightGray : Color.Gray);
                Raylib.DrawText(
                    levelNames[i],
                    (int)(r.X + r.Width / 2 - Raylib.MeasureText(levelNames[i], 20) / 2),
                    (int)(r.Y + (r.Height - 20) / 2),
                    20,
                    Color.Black
                );
            }
        }
    }
}
