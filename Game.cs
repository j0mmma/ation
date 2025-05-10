using Raylib_cs;
using Ation.Common;

namespace Ation.Game
{
    class Game
    {
        public static void Main()
        {
            int screenWidth = Raylib.GetMonitorWidth(0);
            int screenHeight = Raylib.GetMonitorHeight(0);

            Console.WriteLine(screenHeight);
            Console.WriteLine(screenWidth);

            Raylib.InitWindow(screenWidth, screenHeight, "Ation");
            Raylib.SetExitKey(KeyboardKey.Null); // Prevent ESC from auto-closing window
            Raylib.SetTargetFPS(60);

            SceneManager.PushScene(new MainMenuScene());

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.LightGray);

                SceneManager.Update(Raylib.GetFrameTime());
                SceneManager.Render();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
