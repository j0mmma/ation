using Raylib_cs;
using System.Numerics;



namespace Ation.Components;

abstract class Component { }

class RenderComponent : Component
{
    public Vector2 Size { get; set; }
    public Raylib_cs.Color Color { get; set; }
    public RenderComponent(Vector2 size, Raylib_cs.Color color)
    {
        this.Size = size;
        this.Color = color;
    }

}

class PositionComponent : Component
{
    public Vector2 Position { get; set; }
    public PositionComponent(Vector2 position)
    {
        this.Position = position;
    }
}

class SimplePhysicsComponent : Component
{

}

class SimpleColliderComponent : Component
{

}