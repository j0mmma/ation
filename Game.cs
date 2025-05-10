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
            Raylib.SetTargetFPS(60);

            SceneManager.PushScene(new RougelikeScene());

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
