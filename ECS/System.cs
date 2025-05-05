using System.Numerics;
using Raylib_cs;
using Ation.GameWorld;

namespace Ation.Entities
{
    public abstract class BaseSystem
    {
        public abstract string Name { get; set; }
        public abstract void Update(EntityManager em, float dt, World world);
    }

    public class PlayerInputSystem : BaseSystem
    {
        private const float MoveSpeed = 50f;
        private const float JumpVelocity = -100f;

        public override string Name { get; set; } = "PlayerInputSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, _) in em.GetAll<PlayerInputComponent>())
            {
                if (!em.TryGetComponent(entity, out VelocityComponent velocity)) continue;
                if (!em.TryGetComponent(entity, out PositionComponent position)) continue;
                if (!em.TryGetComponent(entity, out SizeComponent size)) continue;

                if (Raylib.IsKeyDown(KeyboardKey.D)) velocity.Velocity.X = MoveSpeed;
                else if (Raylib.IsKeyDown(KeyboardKey.A)) velocity.Velocity.X = -MoveSpeed;
                else velocity.Velocity.X = 0f;

                if (IsGrounded(position.Position, size.Size, world) &&
                    Raylib.IsKeyPressed(KeyboardKey.W))
                {
                    velocity.Velocity.Y = JumpVelocity;
                }
            }
        }

        private bool IsGrounded(Vector2 position, Vector2 size, World world)
        {
            Vector2 probe = new Vector2(position.X + size.X * 0.5f, position.Y + size.Y + 0.05f);
            return world.IsCollidableAt((int)probe.X, (int)probe.Y);
        }
    }

    public class GravitySystem : BaseSystem
    {
        public override string Name { get; set; } = "GravitySystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, gravity) in em.GetAll<GravityComponent>())
            {
                if (em.TryGetComponent(entity, out VelocityComponent velocity))
                {
                    velocity.Velocity.Y += gravity.Gravity * dt;
                }
            }
        }
    }

    public class MovementSystem : BaseSystem
    {
        public override string Name { get; set; } = "MovementSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, velocity) in em.GetAll<VelocityComponent>())
            {
                if (!em.TryGetComponent(entity, out PositionComponent position)) continue;
                if (!em.TryGetComponent(entity, out SizeComponent size)) continue;

                Vector2 pos = position.Position;
                Vector2 moveX = new Vector2(velocity.Velocity.X * dt, 0);
                Vector2 moveY = new Vector2(0, velocity.Velocity.Y * dt);

                TryMove(ref pos, moveX, ref velocity.Velocity, size.Size, world);
                TryMove(ref pos, moveY, ref velocity.Velocity, size.Size, world);

                position.Position = pos;
            }
        }

        private void TryMove(ref Vector2 pos, Vector2 delta, ref Vector2 velocity, Vector2 size, World world)
        {
            Vector2 newPos = pos + delta;
            if (!CollidesAt(newPos, size, world))
            {
                pos = newPos;
            }
            else
            {
                if (delta.X != 0) velocity.X = 0;
                if (delta.Y != 0) velocity.Y = 0;
            }
        }

        private bool CollidesAt(Vector2 pos, Vector2 size, World world)
        {
            int minX = (int)MathF.Floor(pos.X);
            int maxX = (int)MathF.Floor(pos.X + size.X);
            int minY = (int)MathF.Floor(pos.Y);
            int maxY = (int)MathF.Floor(pos.Y + size.Y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (world.IsCollidableAt(x, y)) return true;
                }
            }

            return false;
        }
    }
}
