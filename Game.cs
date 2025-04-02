using System.Numerics;
using Raylib_cs;

using Ation.Systems;
using Ation.Entities;


namespace Ation.Game;

class Game
{
    private const int WINDOW_W = 500;
    private const int WINDOW_H = 500;

    private static EntityManager entityManager = new EntityManager();

    private static Renderer renderer = new Renderer(entityManager, WINDOW_W, WINDOW_H);
    private static ParticleSystem particleSys = new ParticleSystem(entityManager);

    private static EntityTypes selectedMaterial = EntityTypes.SAND;
    private static ParticleSim particleSim = new ParticleSim();

    public static void Main()
    {

        Raylib.InitWindow(WINDOW_W, WINDOW_W, "Ation");

        while (!Raylib.WindowShouldClose())
        {
            ProcessInput();
            Update(Raylib.GetFrameTime());
            // renderer.Update();
            particleSim.Render();
        }

        Raylib.CloseWindow();
    }


    public static void ProcessInput()
    {

        if (Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Left))
        {
            // create a sand entity/particle
            // set RenderComponent.Possition to mousePositon
            Vector2 mousePos = Raylib.GetMousePosition();

            // entityManager.AddEntity(selectedMaterial, Raylib.GetMousePosition());
            particleSim.AddParticle(mousePos);
            Console.WriteLine("Mouse clicked on position: " + mousePos);
        }

    }

    public static void Update(float dt)
    {
        particleSys.Update(dt);
        particleSim.Update(dt);
    }
    public static void Render()
    {

    }
}