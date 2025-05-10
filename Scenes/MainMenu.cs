using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Game
{
    public class MainMenuScene : Scene
    {
        private Rectangle sandboxBtn;
        private Rectangle platformerBtn;
        private Rectangle exitBtn;
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 40;
        private const int ButtonSpacing = 20;

        public MainMenuScene()
        {
            int centerX = Raylib.GetScreenWidth() / 2;
            int centerY = Raylib.GetScreenHeight() / 2;

            float totalHeight = 3 * ButtonHeight + 2 * ButtonSpacing;
            float topY = centerY - totalHeight / 2;

            sandboxBtn = new Rectangle(centerX - ButtonWidth / 2, topY, ButtonWidth, ButtonHeight);
            platformerBtn = new Rectangle(centerX - ButtonWidth / 2, topY + ButtonHeight + ButtonSpacing, ButtonWidth, ButtonHeight);
            exitBtn = new Rectangle(centerX - ButtonWidth / 2, topY + 2 * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);
        }

        public override void ProcessInput()
        {

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                if (SceneManager.SceneCount == 1)
                    Raylib.CloseWindow();
                else
                    SceneManager.PopScene(); // just in case it's stacked (not required if MainMenu is always base)
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                var mouse = Raylib.GetMousePosition();

                if (Raylib.CheckCollisionPointRec(mouse, sandboxBtn))
                    SceneManager.PushScene(new LegacyGameScene());
                else if (Raylib.CheckCollisionPointRec(mouse, platformerBtn))
                    SceneManager.PushScene(new LevelSelectScene());
                else if (Raylib.CheckCollisionPointRec(mouse, exitBtn))
                    Raylib.CloseWindow();
            }
        }

        public override void Update(float dt) { }

        public override void Render()
        {
            string title = "MAIN MENU";
            int titleFontSize = 40;
            int titleWidth = Raylib.MeasureText(title, titleFontSize);

            Raylib.DrawText(
                title,
                Raylib.GetScreenWidth() / 2 - titleWidth / 2,
                (int)(sandboxBtn.Y - 60),
                titleFontSize,
                Color.Black
            );

            DrawButton(sandboxBtn, "Sandbox");
            DrawButton(platformerBtn, "Platformer");
            DrawButton(exitBtn, "Exit");
        }

        private void DrawButton(Rectangle rect, string label)
        {
            bool hovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
            Raylib.DrawRectangleRec(rect, hovered ? Color.Gray : Color.LightGray);
            Raylib.DrawText(
                label,
                (int)(rect.X + rect.Width / 2 - Raylib.MeasureText(label, 20) / 2),
                (int)(rect.Y + (rect.Height - 20) / 2),
                20,
                Color.Black
            );
        }
    }
}
