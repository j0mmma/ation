using System.Numerics;

public abstract class Component
{
    public virtual string Name { get; set; } = "empty_component";
}

public class PositionComponent : Component
{
    public Vector2 Position;
    public PositionComponent(Vector2 position) => Position = position;
}
public class VelocityComponent : Component
{
    public Vector2 Velocity;
    public VelocityComponent(Vector2 velocity) => Velocity = velocity;
}
public class SizeComponent : Component
{
    public Vector2 Size;
    public SizeComponent(Vector2 size) => Size = size;
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