using Raylib_cs;

using Ation.Entities;
using Ation.Components;
namespace Ation.Systems;


abstract class System
{
    protected EntityManager? entityManager;
    public System(EntityManager entityManager)
    {
        this.entityManager = entityManager;
    }

    public abstract void Update(float dt);
}

class Renderer
{
    private EntityManager? entityManager;
    private int windowWidth = 0;
    private int windowHeight = 0;

    public Renderer(EntityManager entityManager, int w, int h)
    {
        this.entityManager = entityManager;
        this.windowHeight = h;
        this.windowWidth = w;
    }

    public void Update()
    {
        var entities = entityManager.GetEntitiesByComponentType(typeof(RenderComponent));


        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.LightGray);

        Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 12, 12, 20, Color.Black);
        Raylib.DrawText($"# of entities: {entityManager.GetSize()}", 12, 25, 20, Color.Black);

        foreach (var e in entities)
        {
            PositionComponent pos = (PositionComponent)e.Value[typeof(PositionComponent)];
            RenderComponent rend = (RenderComponent)e.Value[typeof(RenderComponent)];
            if (pos.Position.X < 0 || pos.Position.X > windowWidth ||
                pos.Position.Y < 0 || pos.Position.Y > windowHeight)
                continue;
            Raylib.DrawRectangle(
                (int)pos.Position.X,
                (int)pos.Position.Y,
                4,
                4,
                rend.Color
            );
        }

        Raylib.EndDrawing();
    }
}

class ParticleSystem : System
{
    public ParticleSystem(EntityManager entityManager) : base(entityManager) { }

    public override void Update(float dt)
    {
        var ents = entityManager.GetEntitiesByComponentType(typeof(SimplePhysicsComponent));

        foreach (var e in ents)
        {
            // get position
            // check if in bounds
            // position.y +=1 if not == to Window_H - 1


        }
    }

    private void ProcessSand(float dt)
    {

    }


}


