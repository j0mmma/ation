using Raylib_cs;
using Ation.Common;

namespace Ation.Game
{
    class Game
    {
        public static void Main()
        {
            Raylib.InitWindow(Variables.WindowWidth, Variables.WindowHeight, "Ation");
            Raylib.SetTargetFPS(60);

            // Start with the legacy simulation scene
            SceneManager.PushScene(new LegacyGameScene());

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
