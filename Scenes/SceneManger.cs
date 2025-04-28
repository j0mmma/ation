using System.Collections.Generic;

namespace Ation.Game
{
    public static class SceneManager
    {
        private static Stack<Scene> scenes = new();

        public static void PushScene(Scene scene)
        {
            scenes.Push(scene);
        }

        public static void PopScene()
        {
            if (scenes.Count > 0)
                scenes.Pop();
        }

        public static Scene CurrentScene => scenes.Count > 0 ? scenes.Peek() : null;

        public static void Update(float dt)
        {
            if (CurrentScene != null)
            {
                CurrentScene.ProcessInput();
                CurrentScene.Update(dt);
            }
        }

        public static void Render()
        {
            if (CurrentScene != null)
            {
                CurrentScene.Render();
            }
        }

        public static void Clear()
        {
            scenes.Clear();
        }
    }
}
