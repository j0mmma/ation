using System.Numerics;
using Raylib_cs;

namespace Ation.Entities;

public abstract class Component
{
    public virtual string Name { get; set; } = "empty_component";
}

public class TransformComponent : Component
{
    public Vector2 Position; //center-bottom of the entity
    public float Scale = 1f;
    public float Rotation = 0f;

    public TransformComponent(Vector2 position, float scale = 1f)
    {
        Position = position;
        Scale = scale;
    }
}

public class VelocityComponent : Component
{
    public Vector2 Velocity;
    public VelocityComponent(Vector2 velocity) => Velocity = velocity;
}

public class PlayerInputComponent : Component { }
public class GravityComponent : Component
{
    public float Gravity;
    public GravityComponent(float gravity) => Gravity = gravity;
}
public class ColliderComponent : Component
{
    public Vector2 Size;
    public Vector2 Offset;
    public bool IsGrounded;

    public ColliderComponent(Vector2 size, Vector2 offset = default)
    {
        Size = size;
        Offset = offset;
    }


}
public class MovementIntentComponent : Component
{
    public Vector2 Delta;
}



public class RenderableComponent : Component
{
    public Texture2D Texture;
    public Rectangle Source;
    public Vector2 Offset;
    public Color Tint;
    public float Scale;

    public RenderableComponent(Texture2D texture, Rectangle source, Vector2 offset = default, float scale = 1.0f, Color? tint = null)
    {
        Texture = texture;
        Source = source;
        Offset = offset;
        Scale = scale;
        Tint = tint ?? Color.White;
    }
}

