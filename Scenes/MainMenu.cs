using Raylib_cs;
using Ation.Common;

namespace Ation.Game
{
    public class MainMenuScene : Scene
    {
        public override void ProcessInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                SceneManager.PushScene(new LegacyGameScene());
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                Raylib.CloseWindow();
            }
        }

        public override void Update(float dt)
        {
            // No update logic needed for now
        }

        public override void Render()
        {
            int screenWidth = Variables.WindowWidth;
            int screenHeight = Variables.WindowHeight;

            Raylib.DrawText(
                "MAIN MENU",
                screenWidth / 2 - Raylib.MeasureText("MAIN MENU", 40) / 2,
                screenHeight / 2 - 60,
                40,
                Color.Black
            );

            Raylib.DrawText(
                "Press [ENTER] to Start",
                screenWidth / 2 - Raylib.MeasureText("Press [ENTER] to Start", 20) / 2,
                screenHeight / 2 + 10,
                20,
                Color.DarkGray
            );

            Raylib.DrawText(
                "Press [ESC] to Quit",
                screenWidth / 2 - Raylib.MeasureText("Press [ESC] to Quit", 20) / 2,
                screenHeight / 2 + 40,
                20,
                Color.DarkGray
            );
        }
    }
}
