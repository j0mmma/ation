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

public enum CollisionType
{
    None,       // ignored
    Passive,    // collides with world but doesn't block entities
    Solid       // blocks movement (e.g., player, walls)
}

public class ColliderComponent : Component
{
    public Vector2 Size;
    public Vector2 Offset;
    public bool IsGrounded;
    public bool IsInLiquid;
    public CollisionType CollisionType;

    public ColliderComponent(Vector2 size, Vector2 offset = default, CollisionType type = CollisionType.Solid)
    {
        Size = size;
        Offset = offset;
        CollisionType = type;
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

public class StateComponent : Component
{
    public bool IsInLiquid;
    public bool IsInLava;
    public bool IsOnFire;

    // Optional timers or intensity levels
    public float FireDuration = 0f;
}

public class ItemComponent : Component
{
    public Action<EntityManager, Entity, Entity, Vector2>? UseAction { get; }

    public ItemComponent(Action<EntityManager, Entity, Entity, Vector2>? useAction = null)
    {
        Name = "item";
        UseAction = useAction;
    }
}

public class PickupableComponent : Component
{
    public PickupableComponent()
    {
        Name = "pickupable";
    }
}

public class InventoryComponent : Component
{
    public Entity?[] Slots = new Entity?[3];
    public int SelectedIndex = 0;
}
public class DropCooldownComponent : Component
{
    public float TimeRemaining;

    public DropCooldownComponent(float duration = 1f)
    {
        TimeRemaining = duration;
    }
}