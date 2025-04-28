namespace Ation.Game
{
    public abstract class Scene
    {
        public abstract void Update(float dt);
        public abstract void ProcessInput();
        public abstract void Render();
    }
}
