using Raylib_cs;
using System.Numerics;
using Ation.Components;

namespace Ation.Entities;


enum EntityTypes
{
    UI_Button,
    NPC,
    PLAYER,

    SAND,
    WATER,
    WALL

}

enum ParticleTypes
{
    SAND,
    WATER,
    WALL
}

class Entity { public string? ID { get; set; } }



class EntityManager
{
    private Dictionary<int, Dictionary<Type, Component>> entityComponents = new Dictionary<int, Dictionary<Type, Component>>();
    private int nextEntityId = 1;
    public EntityManager()
    {

    }
    // passing position here looks cursed
    public void AddEntity(EntityTypes type, Vector2 position)
    {
        switch (type)
        {
            case EntityTypes.SAND:
                AddSand(position);
                nextEntityId += 1;
                break;

        }
    }

    private void AddSand(Vector2 position)
    {
        var posComponent = new PositionComponent(position);
        var renderComponent = new RenderComponent(new Vector2(4.0f, 4.0f), Raylib_cs.Color.Yellow);
        var physics = new SimplePhysicsComponent();


        Dictionary<Type, Component> entComponents = new Dictionary<Type, Component>();

        entComponents.Add(posComponent.GetType(), posComponent);
        entComponents.Add(renderComponent.GetType(), renderComponent);
        entComponents.Add(physics.GetType(), physics);

        entityComponents.Add(nextEntityId, entComponents);

    }

    public Dictionary<Type, Component> this[int key]
    {
        get => entityComponents[key];
        set => entityComponents[key] = value;
    }

    public int GetSize()
    {
        return entityComponents.Count();
    }

    public KeyValuePair<int, Dictionary<Type, Component>> GetLastEntity() { return entityComponents.LastOrDefault(); }

    public void RemoveEntity(string ID)
    {

    }
    public void addComponent(int EntityID, Component component)
    {

    }
    public void removeComponent(string EntityID, Component component) { }
    public Dictionary<int, Dictionary<Type, Component>> GetEntitiesByComponentType(Type componentType)
    {
        var res = new Dictionary<int, Dictionary<Type, Component>>();
        foreach (var e in entityComponents)
        {
            if (e.Value.ContainsKey(componentType))
                res.Add(e.Key, e.Value);
        }
        return res;
    }
}
