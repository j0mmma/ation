using System.IO;
using System.Numerics;
using System.Text.Json;
using Raylib_cs;
using Ation.Common;

namespace Ation.Game
{
    public class GameOverScene : Scene
    {
        private readonly float timeSurvived;
        private readonly int enemiesKilled;
        private readonly string previousMapName;
        private readonly string previousMapPath;
        private ScoreRecord? bestScore;

        private Rectangle retryBtn;
        private Rectangle menuBtn;
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 40;
        private const int ButtonSpacing = 20;

        public GameOverScene(float timeSurvived, int enemiesKilled, string mapName)
        {
            this.timeSurvived = timeSurvived;
            this.enemiesKilled = enemiesKilled;
            this.previousMapName = mapName;
            this.previousMapPath = $"Assets/Levels/{mapName}.json";
            LoadBestScore(mapName);

            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            float topY = screenHeight / 2f + 30;

            retryBtn = new Rectangle(screenWidth / 2f - ButtonWidth / 2, topY, ButtonWidth, ButtonHeight);
            menuBtn = new Rectangle(screenWidth / 2f - ButtonWidth / 2, topY + ButtonHeight + ButtonSpacing, ButtonWidth, ButtonHeight);
        }

        private void LoadBestScore(string mapName)
        {
            string scoreFile = "Assets/score.json";
            if (!File.Exists(scoreFile)) return;

            try
            {
                var json = File.ReadAllText(scoreFile);
                var records = JsonSerializer.Deserialize<List<ScoreRecord>>(json);
                bestScore = records?.FirstOrDefault(r => Path.GetFileNameWithoutExtension(r.Map) == mapName);
            }
            catch
            {
                bestScore = null;
            }
        }

        public override void ProcessInput()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                SceneManager.PopScene();
                return;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mouse = Raylib.GetMousePosition();

                if (Raylib.CheckCollisionPointRec(mouse, retryBtn))
                {
                    SceneManager.PopScene();
                    SceneManager.PushScene(new RougelikeScene(previousMapPath));
                }
                else if (Raylib.CheckCollisionPointRec(mouse, menuBtn))
                {
                    SceneManager.PopScene();
                }
            }
        }

        public override void Update(float dt) { }

        public override void Render()
        {
            int screenWidth = Raylib.GetScreenWidth();
            int centerX = screenWidth / 2;

            string title = "GAME OVER";
            int titleFontSize = 40;
            int titleWidth = Raylib.MeasureText(title, titleFontSize);
            Raylib.DrawText(title, centerX - titleWidth / 2, 100, titleFontSize, Color.Red);

            string timeStr = $"Time: {timeSurvived:F1}s";
            string killsStr = $"Kills: {enemiesKilled}";
            string mapStr = $"Map: {previousMapName}";

            Raylib.DrawText(mapStr, centerX - Raylib.MeasureText(mapStr, 20) / 2, 160, 20, Color.Black);
            Raylib.DrawText(timeStr, centerX - Raylib.MeasureText(timeStr, 20) / 2, 190, 20, Color.Black);
            Raylib.DrawText(killsStr, centerX - Raylib.MeasureText(killsStr, 20) / 2, 220, 20, Color.Black);

            if (bestScore != null)
            {
                string bestTime = $"Best Time: {bestScore.TimeSurvived:F1}s";
                string bestKills = $"Best Kills: {bestScore.EnemiesKilled}";

                Raylib.DrawText(bestTime, centerX - Raylib.MeasureText(bestTime, 18) / 2, 260, 18, Color.DarkGray);
                Raylib.DrawText(bestKills, centerX - Raylib.MeasureText(bestKills, 18) / 2, 280, 18, Color.DarkGray);
            }

            DrawButton(retryBtn, "Retry");
            DrawButton(menuBtn, "Main Menu");
        }

        private void DrawButton(Rectangle rect, string label)
        {
            bool hovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);
            Raylib.DrawRectangleRec(rect, hovered ? Color.LightGray : Color.Gray);
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
